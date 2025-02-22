using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using TelegramPartHook.Domain.Aggregations.ConfigAggregation;
using TelegramPartHook.Domain.Aggregations.SearchCacheAggregation;
using TelegramPartHook.Domain.Aggregations.UserAggregation;
using TelegramPartHook.Domain.Constants;
using TelegramPartHook.Domain.SeedWork;
using TelegramPartHook.Infrastructure.Persistence;
using TelegramPartHook.Infrastructure.Persistence.Repositories;

namespace TelegramPartHook.DI;

public static class PersistenceDI
{
    public static IServiceCollection AddPersistence(this IServiceCollection services,
        IAdminConfiguration adminConfiguration)
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(adminConfiguration.ConnectionString)
            .EnableDynamicJson()
            .Build();

        services.AddDbContext<BotContext>(op => op.UseNpgsql(dataSourceBuilder));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        //repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ISearchCacheRepository, SearchCacheRepository>();

        return services;
    }

    public static void ApplyMigrationsOnDatabase(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var context = scope.ServiceProvider.GetService<BotContext>();
        var adminConfiguration = scope.ServiceProvider.GetService<IAdminConfiguration>();
        try
        {
            context.Database.Migrate();
            TrySeedInitialData(context, adminConfiguration);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private static void TrySeedInitialData(BotContext context, IAdminConfiguration adminConfiguration)
    {
        var user = context.Set<User>().AsNoTracking()
            .FirstOrDefault(u => u.telegramid == adminConfiguration.AdminChatId);

        if (user is null)
        {
            user = new User(adminConfiguration.AdminChatId, "Rogim Nazario")
                .Upgrade("rogim", DateTime.Now.AddYears(1).ToString("dd/MM/yyyy"));

            context.Add(user);
        }

        var configs = context.Set<Config>().AsNoTracking().ToArray();

        var tomorrow = DateTime.UtcNow.AddDays(1);
        if (configs.Length == 0)
        {
            context.AddRange(
                Config.CreateDateTime(tomorrow, ConfigDateTimeName.NextDateSearchOnInstagram),
                Config.CreateDateTime(tomorrow, ConfigDateTimeName.NextDateToMonitorRun),
                Config.CreateDateTime(tomorrow, ConfigDateTimeName.NextDateToCacheClear),
                Config.Create(15, Config.MinutesToClean));
        }

        var emptyCaches = context
            .Set<SearchCache>()
            .Where(c => string.IsNullOrEmpty(c.AddedDate))
            .ToArray();

        foreach (var caches in emptyCaches)
        {
            caches.AddedDate = DateTime.UtcNow.ToString(DateConstants.DatabaseFormat);
        }

        context.SaveChanges();
    }
}