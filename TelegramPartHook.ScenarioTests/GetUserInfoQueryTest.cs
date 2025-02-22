using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramPartHook.Application.Factories;
using TelegramPartHook.Application.Queries;
using TelegramPartHook.Application.Services;
using TelegramPartHook.Domain.Aggregations.UserAggregation;
using TelegramPartHook.Domain.Constants;
using TelegramPartHook.Tests.Core.Fixtures;
using TelegramPartHook.Tests.Core.Helpers;
using Xunit;

namespace TelegramPartHook.ComponentTests;

public class GetUserInfoQueryTest(CoreDependencyInjectionFixture fixture)
    : BaseCommandTest(fixture), IClassFixture<CoreDependencyInjectionFixture>
{
    [Fact]
    public async Task GetUserInfoQueryHandler()
    {
        var userRepository = ServiceProvider.GetRequiredService<IUserRepository>();

        var sender = new Mock<ITelegramSender>();
        var accessor = new Mock<ISearchAccessor>();
        var config = new Mock<IAdminConfiguration>();

        var message = $"/info {TestHelper.AdminId}";
        accessor.Setup(s => s.CurrentSearch())
            .Returns(new Application.DTO.Search(message, TestHelper.AdminUser));
        config.Setup(s => s.IsUserAdmin(It.IsAny<User>())).Returns(true);

        var query = new GetUserInfoQuery();

        var handler = new GetUserInfoQueryHandler(userRepository, sender.Object, config.Object, accessor.Object);

        await handler.Handle(query, CancellationToken.None);

        sender.Verify(v => v.SendToAdminAsync(It.Is<string>(m => m.StartsWith($"*ID*: {TestHelper.AdminId}\n")),
            It.IsAny<CancellationToken>(), It.IsAny<ParseMode>(), It.IsAny<InlineKeyboardMarkup>()));
    }
}