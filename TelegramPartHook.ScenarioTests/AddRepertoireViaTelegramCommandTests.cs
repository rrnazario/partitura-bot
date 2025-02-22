using Light.GuardClauses.FrameworkExtensions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using TelegramPartHook.Application.Commands.Repertoire;
using TelegramPartHook.Application.DTO;
using TelegramPartHook.Application.Factories;
using TelegramPartHook.Application.Services;
using TelegramPartHook.Domain.Aggregations.SearchCacheAggregation;
using TelegramPartHook.Domain.Aggregations.UserAggregation;
using TelegramPartHook.Domain.Constants;
using TelegramPartHook.Domain.Exceptions;
using TelegramPartHook.Domain.SeedWork;
using TelegramPartHook.Infrastructure.Persistence;
using TelegramPartHook.Tests.Core.Fixtures;
using Xunit;

namespace TelegramPartHook.ComponentTests;

public class AddRepertoireViaTelegramCommandTests : IClassFixture<CoreDependencyInjectionFixture>
{
    private readonly Mock<ITelegramSender> _sender;
    private readonly BotContext _context;
    private readonly IUserRepository _userRepository;
    private readonly ISearchCacheRepository _searchCacheRepository;
    private readonly IAdminConfiguration _adminConfiguration;
    private readonly Mock<ISearchAccessor> _searchAccessor;

    public AddRepertoireViaTelegramCommandTests(CoreDependencyInjectionFixture fixture)
    {
        var serviceProvider = fixture.ServiceProvider;

        _sender = new();
        _searchAccessor = new();
        _context = serviceProvider.GetRequiredService<BotContext>();
        _userRepository = serviceProvider.GetRequiredService<IUserRepository>();
        _searchCacheRepository = serviceProvider.GetRequiredService<ISearchCacheRepository>();
        _adminConfiguration = serviceProvider.GetRequiredService<IAdminConfiguration>();
    }

    [Fact]
    public async Task AddRepertoireWithMaxSheets_WhenUserIsNotVip_ShouldThrowNotVipException()
    {
        var user = new User("12345", "not vip user");

        user.InitializeRepertoire();
        Enumerable.Range(0, _adminConfiguration.MaxFreeSheetsOnRepertoire + 1)
            .ForEach((f) =>
                user.Repertoire.TryAdd(new SheetSearchResult($"http://{Guid.NewGuid()}.jpg",
                    Enums.FileSource.Crawler)));

        _context.Add(user);
        await _context.SaveChangesAsync();

        _searchAccessor.Setup(s => s.CurrentSearch()).Returns(new Search("max_exception", user));

        var target = CreateTarget();

        var action = () => target.Handle(new AddRepertoireSingleImageViaTelegramCommand(), CancellationToken.None)
            .GetAwaiter().GetResult();

        action.Should().Throw<NotVipUserException>();
    }

    private AddRepertoireSingleImageViaTelegramCommandHandler CreateTarget()
        => new(_searchCacheRepository, _userRepository, _sender.Object, _searchAccessor.Object, _adminConfiguration);
}