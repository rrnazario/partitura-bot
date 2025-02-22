using TelegramPartHook.Application.Commands.UpgradeUser;

namespace TelegramPartHook.Application.UnitTests.Commands;

public class UpgradeUserInfoCommandTest
{
    [Fact]
    public void UpgradeUserInvalidInfoTest()
    {
        Action action = () => new UpgradeUserInfo(new[] { "invalidinfo" });

        action.Should().Throw<Exception>()
            .And.Message.Should().Be("Invalid upgrade info");
    }

    [Fact]
    public void UpgradeUserInvalidExpiredDateInfoTest()
    {
        Action action = () => new UpgradeUserInfo(new[] { "00001111", "portaluser" ,"invaliddate" });

        action.Should().Throw<Exception>()
            .And.Message.Should().Contain("is an invalid expired date info");
    }
}