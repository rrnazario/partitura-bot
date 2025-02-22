using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Npgsql;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramPartHook.Application.Commands;
using TelegramPartHook.Application.Factories;
using TelegramPartHook.Application.Helpers;
using TelegramPartHook.Application.Interfaces;
using TelegramPartHook.Application.Interfaces.Searches;
using TelegramPartHook.Application.Services;
using TelegramPartHook.Application.Services.Searches;
using TelegramPartHook.Application.Services.Searches.Crawlers;
using TelegramPartHook.Domain.Aggregations.ConfigAggregation;
using TelegramPartHook.Domain.Aggregations.SearchCacheAggregation;
using TelegramPartHook.Domain.Aggregations.UserAggregation;
using TelegramPartHook.Domain.Constants;
using TelegramPartHook.Domain.SeedWork;
using TelegramPartHook.Infrastructure.Helpers;
using TelegramPartHook.Infrastructure.Persistence;
using TelegramPartHook.Infrastructure.Persistence.Repositories;
using TelegramPartHook.Tests.Core.Docker;
using TelegramPartHook.Tests.Core.Helpers;

namespace TelegramPartHook.Tests.Core.Fixtures;

public class CoreDependencyInjectionFixture : IDisposable
{
    public DockerDatabaseSetup PostgresSetup { get; }
    public CoreDependencyInjectionFixture()
    {
        PostgresSetup = new DockerDatabaseSetup();
        ServiceProvider = CreateDefaultInjections().BuildServiceProvider();

        TryApplyMigrations();
    }
        
    private IServiceCollection CreateDefaultInjections()
    {
        PostgresSetup.InitializeAsync().GetAwaiter().GetResult();

        var services = new ServiceCollection();

        services.AddHttpClient();

        services.AddSingleton(AddTelegramSender);

        services.AddSingleton(sp =>
        {
            var logMock = new Mock<ILogHelper>();

            return logMock.Object;
        });

        services.AddSingleton(_ =>new Mock<IDropboxService>().Object);
        services.AddSingleton(_ => new Mock<ISystemHelper>().Object);
        services.AddSingleton<IGlobalState, GlobalState>();

        var configuration = new ConfigurationBuilder()
            .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
            .Build();

        services.AddSingleton<IConfiguration>(_ => configuration);

        var adminConfiguration = new AdminConfiguration(configuration);
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(PostgresSetup.ConnectionString())
            .EnableDynamicJson()
            .Build();

        services.AddDbContext<BotContext>(op => op.UseNpgsql(dataSourceBuilder));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        //repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ISearchCacheRepository, SearchCacheRepository>();

        services.AddSingleton<IAdminConfiguration>(_ => adminConfiguration);
        services.AddScoped<ISearchAccessor, SearchAccessor>();
        services.AddScoped<ISanitizeService, SanitizeService>();

        services.AddSingleton(s =>
        {
            var cacheMock = new Mock<IMemoryCache>();
            cacheMock.Setup(s => s.TryGetValue(It.IsAny<object>(), out It.Ref<object>.IsAny!));
            cacheMock.Setup(s => s.CreateEntry(It.IsAny<object>())).Returns(new Mock<ICacheEntry>().Object);
            cacheMock.Setup(s => s.Remove(It.IsAny<object>()));

            return cacheMock.Object;
        });

        //factories
        services.AddScoped<IRequestFactory, RequestFactory>();
        services.AddScoped<ISearchFactory, SearchFactory>();
        services.AddScoped<IUserFactory, UserFactory>();

        //Searches
        services.AddSingleton<ICacheSearchService, CacheSearchService>();
        services.AddSingleton<IDropboxService, DropboxService>();
        services.AddSingleton<IChoroSearchService, ChoroSearchService>();

        //Crawlers
        services.AddSingleton<ICrawlerSearchService, CrawlerSearchService>();
        services.AddSingleton<IBlogspotCrawlerSearchService, BlogspotCrawlerSearchService>();
        services.AddSingleton<INandinhoCrawlerSearchService, NandinhoCrawlerSearchService>();
        services.AddSingleton<IPagodeAudioPartituraCrawlerSearchService, PagodeAudioPartituraCrawlerSearchService>();
        services.AddSingleton<IBrasilSonoroCrawlerService, BrasilSonoroCrawlerService>();

        services.AddSingleton<IInstagramCrawlerSearchService, InstagramCrawlerSearchService>();


        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies()));
        AddMediatorCommands(services);

        return services;
    }

    protected virtual ITelegramSender AddTelegramSender(IServiceProvider provider)
    {
        var senderMock = new Mock<ITelegramSender>();

        senderMock.Setup(s => s.SendToAdminAsync(It.IsAny<string>(), It.IsAny<CancellationToken>(),
            It.IsAny<ParseMode>(), It.IsAny<InlineKeyboardMarkup>()));

        senderMock.Setup(s =>
            s.SendPhotoAsync(It.IsAny<string>(), It.IsAny<InputFile>(), It.IsAny<CancellationToken>(), It.IsAny<string>(), It.IsAny<InlineKeyboardMarkup>()));

        return senderMock.Object;
    }


    private void TryApplyMigrations()
    {
        const int total = 10;
        var attempt = 0;

        using var scope = ServiceProvider.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<BotContext>();
        do
        {
            try
            {                    
                ctx.Database.Migrate();
                SeedInitialData(ctx);
                break;
            }
            catch (Exception e)
            {
                try
                {
                    Console.WriteLine(e);
                    ctx.Database.EnsureCreated();
                }
                catch (Exception)
                {
                    Task.Delay(TimeSpan.FromSeconds(1));
                }
            }
        } while (attempt++ < total);            
    }

    private static void SeedInitialData(BotContext context)
    {
        context.Add(TestHelper.AdminUser);
            
        var tomorrow = DateTime.UtcNow.AddDays(1);

        context.AddRange(
            Config.CreateDateTime(tomorrow, ConfigDateTimeName.NextDateSearchOnInstagram),
            Config.CreateDateTime(tomorrow, ConfigDateTimeName.NextDateToMonitorRun),
            Config.CreateDateTime(tomorrow, ConfigDateTimeName.NextDateToCacheClear),
            Config.Create(15, Config.MinutesToClean));

        context.SaveChanges();
    }

    public void Dispose()
    {
        PostgresSetup?.DisposeAsync().GetAwaiter().GetResult();
    }

    public ServiceProvider ServiceProvider { get; }

    private IServiceCollection AddMediatorCommands(IServiceCollection services)
    {
        var type = typeof(IBotRequest);
        var types = Assembly.GetAssembly(typeof(UnsubscribeCommand))!
            .GetTypes()
            .Where(w => type.IsAssignableFrom(w) && !w.IsAbstract);

        foreach (var refType in types)
        {
            services.AddScoped(type, refType);
        }

        return services;
    }
}