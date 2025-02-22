using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramPartHook.Application.Commands;
using TelegramPartHook.Application.DTO;
using TelegramPartHook.Application.Factories;
using TelegramPartHook.Application.Services;
using TelegramPartHook.Domain.Aggregations.UserAggregation;
using TelegramPartHook.Domain.Constants;
using TelegramPartHook.Domain.Helpers;
using TelegramPartHook.Domain.SeedWork;
using TelegramPartHook.Infrastructure.Persistence;
using TelegramPartHook.Tests.Core.Fixtures;
using TelegramPartHook.Tests.Core.Helpers;
using Xunit;

namespace TelegramPartHook.ComponentTests;

public class RememberHandlingCommandTest : IClassFixture<CoreDependencyInjectionFixture>
{
    private IMemoryCache _cache;
    private BotContext _context;
    private IUnitOfWork _uow;
    private IAdminConfiguration _adminConfiguration;

    public RememberHandlingCommandTest(CoreDependencyInjectionFixture fixture)
    {
        var serviceProvider = fixture.ServiceProvider;

        _cache = serviceProvider.GetRequiredService<IMemoryCache>();
        _context = serviceProvider.GetRequiredService<BotContext>();
        _uow = serviceProvider.GetRequiredService<IUnitOfWork>();
        _adminConfiguration = serviceProvider.GetRequiredService<IAdminConfiguration>();
    }

    [Fact]
    public async Task GetAllRemembers()
    {
        const string getAllRemembers = "/lembretes";
        const string searchScheduledText = "sambarril|18/08/2022";

        var user = TestHelper.GenerateUser(_context);

        var monitoredItem = new MonitoredItem(searchScheduledText);

        var sender = new Mock<ITelegramSender>();

        await _uow.ExecuteSqlRawAsync(
            $"UPDATE client SET searchesScheduled = '{searchScheduledText}' WHERE telegramId = '{user.telegramid}'");
        user = await _context.Set<User>().AsNoTracking().FirstAsync(_ => _.telegramid == user.telegramid);

        var search = new Search(getAllRemembers, user);
        var req = new RememberHandlingCommand(search);
        var handler = CreateTarget(sender.Object);
        var expectedMessage = MessageHelper.GetMessage(search.User.culture, Enums.MessageName.ChoseRemember);

        //Act
        await handler.Handle(req, CancellationToken.None);

        //Assert
        sender.Verify(s => s.SendTextMessageAsync(user.telegramid,
            expectedMessage,
            It.IsAny<CancellationToken>(),
            It.IsAny<ParseMode>(),
            It.Is<InlineKeyboardMarkup>(mkp =>
                mkp.InlineKeyboard.Any(btnLine => btnLine.First().Text.Equals(monitoredItem.Format())))));
    }

    [Fact]
    public async Task ClientWithoutRemember_TryListAll_SeeProperMessage()
    {
        const string getAllRemembers = "/lembretes";
        var user = TestHelper.GenerateUser(_context);

        //Arrange
        var sender = new Mock<ITelegramSender>();

        await _uow.ExecuteSqlRawAsync(
            $"UPDATE client SET searchesScheduled = '' WHERE telegramId = '{user.telegramid}'");
        user = await _context.Set<User>().AsNoTracking().FirstAsync(_ => _.telegramid == user.telegramid);

        var search = new Search(getAllRemembers, user);
        var req = new RememberHandlingCommand(search);
        var handler = CreateTarget(sender.Object);
        var expectedMessage = MessageHelper.GetMessage(search.User.culture, Enums.MessageName.ThereAreNoRemembers);

        //Act
        await handler.Handle(req, CancellationToken.None);

        //Assert
        sender.Verify(s => s.SendTextMessageAsync(
            It.IsAny<string>(),
            expectedMessage,
            It.IsAny<CancellationToken>(),
            It.IsAny<ParseMode>(),
            It.IsAny<InlineKeyboardMarkup>()), Times.Once);
    }

    [Fact]
    public async Task ChooseMonitoredItem_ShowProperMessage()
    {
        const string getAllRemembers = "/remover 0";
        const string searchScheduledText = "sambarril|18/08/2022";

        var user = TestHelper.GenerateUser(_context);

        //Arrange
        var monitoredItem = new MonitoredItem(searchScheduledText);

        //mocks
        var sender = new Mock<ITelegramSender>();

        await _uow.ExecuteSqlRawAsync(
            $"UPDATE client SET searchesScheduled = '{searchScheduledText}' WHERE telegramId = '{user.telegramid}'");
        user = await _context.Set<User>().AsNoTracking().FirstAsync(_ => _.telegramid == user.telegramid);

        var search = new Search(getAllRemembers, user);
        var req = new RememberHandlingCommand(search);
        req.SetNextState(RememberHandlingState.RememberSelected);
        req.Rehydrate(search.Term);

        var handler = CreateTarget(sender.Object);
        var expectedMessage = MessageHelper.GetMessage(search.User.culture, Enums.MessageName.ConfirmRememberExclusion,
            monitoredItem.Format());

        //Act
        await handler.Handle(req, CancellationToken.None);

        //Assert
        sender.Verify(s => s.EditMessageTextAsync(user.telegramid,
            It.IsAny<int>(),
            expectedMessage,
            It.IsAny<CancellationToken>(),
            It.IsAny<ParseMode>(),
            It.IsAny<InlineKeyboardMarkup>()), Times.Once);
    }

    private RememberHandlingCommandHandler CreateTarget(ITelegramSender sender)
    {
        return new RememberHandlingCommandHandler(_cache, sender, _context, _adminConfiguration,
            new Mock<ISearchAccessor>().Object);
    }
}