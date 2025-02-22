using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramPartHook.Application.Commands;
using TelegramPartHook.Application.DTO;
using TelegramPartHook.Application.Factories;
using TelegramPartHook.Application.Services;
using TelegramPartHook.Domain.Constants;
using TelegramPartHook.Domain.Helpers;
using TelegramPartHook.Tests.Core.Fixtures;
using TelegramPartHook.Tests.Core.Helpers;
using Xunit;


namespace TelegramPartHook.ComponentTests;

public class SendToAdminCommandTest 
    : BaseCommandTest, IClassFixture<CoreDependencyInjectionFixture>
{
    private IMemoryCache _cache;
    private IAdminConfiguration _adminConfiguration;    

    public SendToAdminCommandTest(CoreDependencyInjectionFixture fixture) : base(fixture)
    {
        _cache = ServiceProvider.GetRequiredService<IMemoryCache>();
        _adminConfiguration = ServiceProvider.GetRequiredService<IAdminConfiguration>();
    }

    [InlineData("/suggestion")]
    [InlineData("/ask")]
    [Theory]
    public async Task SendMessageToAdminSuccessfully(string cmd)
    {
        //Arrange
        var msg = "Amor amigo";
        var sendSuggestion = $"{cmd} {msg}";
        var commandText = TestHelper.GenerateCommandText(sendSuggestion);
        //mocks
        var sender = new Mock<ITelegramSender>();

        var search = await CreateSearchAsync(commandText);
        var req = new SendAdminMessageCommand(search);
        req.SetNextState(SendAdminMessageState.ConfirmReceived);
        req.Rehydrate(search.Term);

        var handler = CreateTarget(sender.Object);
        var expectedMessage = MessageHelper.GetMessage(search.User.culture, Enums.MessageName.MessageSuccessfullySentToAdmin);

        //Act
        await handler.Handle(req, CancellationToken.None);

        //Assert
        sender.Verify(v => v.SendTextMessageAsync(It.IsAny<string>(),
                                                 expectedMessage,
                                                 It.IsAny<CancellationToken>(),
                                                 It.IsAny<ParseMode>(),
                                                 It.IsAny<InlineKeyboardMarkup>()));
    }

    private SendAdminMessageCommandHandler CreateTarget(ITelegramSender? sender = null) 
    => new(_cache, sender!, _adminConfiguration, new Mock<ISearchAccessor>().Object);

    private async Task<Search> CreateSearchAsync(string commandText)
    {
        var searchFactory = ServiceProvider.GetRequiredService<ISearchFactory>();

        return await searchFactory.CreateSearchAsnc(commandText);
    }
}