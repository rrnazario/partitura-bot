using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TelegramPartHook.Application.Commands.UpgradeUser;
using TelegramPartHook.Application.DTO;
using TelegramPartHook.Application.Factories;
using TelegramPartHook.Application.Services;
using TelegramPartHook.Domain.Aggregations.UserAggregation;
using TelegramPartHook.Domain.Constants;
using TelegramPartHook.Domain.Exceptions;
using TelegramPartHook.Infrastructure.Persistence;
using TelegramPartHook.Tests.Core.Fixtures;
using TelegramPartHook.Tests.Core.Helpers;
using Xunit;


namespace TelegramPartHook.ComponentTests;

public class UpgradeUserCommandTest
    : BaseCommandTest, IClassFixture<CoreDependencyInjectionFixture>
{
    private readonly IUserRepository _userRepository;
    private readonly ITelegramSender _sender;
    private readonly ISearchAccessor _searchAccessor;
    private readonly Mock<IAdminConfiguration> _adminConfiguration;

    public UpgradeUserCommandTest(CoreDependencyInjectionFixture fixture) : base(fixture)
    {
        _userRepository = ServiceProvider.GetRequiredService<IUserRepository>();
        _sender = ServiceProvider.GetRequiredService<ITelegramSender>();
        _searchAccessor = ServiceProvider.GetRequiredService<ISearchAccessor>();

        _adminConfiguration = new Mock<IAdminConfiguration>();
        _adminConfiguration.Setup(s => s.IsUserAdmin(It.IsAny<User>())).Returns(true);
    }

    [Fact]
    public async Task UpgradeUserThatDoesNotExist()
    {
        var cmd = $"/up 11112222 non_existent_user {DateTime.UtcNow.AddYears(1):dd/MM/yyyy}";

        var commandText = TestHelper.GenerateAdminCommandText(cmd);
        _searchAccessor.SetCurrentSearch(await CreateSearchAsync(commandText));
        var req = new UpgradeUserCommand();

        var target = CreateTarget();

        Action action = () => target.Handle(req, CancellationToken.None).GetAwaiter().GetResult();

        action.Should().Throw<UserNotFoundException>();
    }

    [Fact]
    public async Task UpgradeUserTest()
    {
        var portalUser = "rogimUpgrade";
        var expirationDate = $"{DateTime.UtcNow.AddYears(1):dd/MM/yyyy}";

        var cmd = $"/up {TestHelper.AdminId} {portalUser} {expirationDate}";

        var commandText = TestHelper.GenerateAdminCommandText(cmd);
        _searchAccessor.SetCurrentSearch(await CreateSearchAsync(commandText));
        var req = new UpgradeUserCommand();

        var target = CreateTarget();

        await target.Handle(req, CancellationToken.None);

        var expectedUser = await _userRepository.GetByIdAsync(TestHelper.AdminId);
        expectedUser!.isvip.Should().BeTrue();
        expectedUser.vipinformation.Should().StartWith(expirationDate);
        expectedUser.vipinformation.Should().EndWith($"\n{portalUser}");
    }

    private UpgradeUserCommandHandler CreateTarget() => new(_userRepository, _sender, _searchAccessor, _adminConfiguration.Object);

    private async Task<Search> CreateSearchAsync(string commandText)
    {
        var searchFactory = ServiceProvider.GetRequiredService<ISearchFactory>();

        return await searchFactory.CreateSearchAsnc(commandText);
    }
}