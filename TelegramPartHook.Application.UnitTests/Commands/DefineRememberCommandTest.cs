using TelegramPartHook.Application.Factories;
using TelegramPartHook.Application.Services;
using TelegramPartHook.Domain.SeedWork;
using TelegramPartHook.Infrastructure.Persistence;
using TelegramPartHook.Tests.Core.Helpers;

namespace TelegramPartHook.Application.UnitTests.Commands;

public class DefineRememberCommandTest
{
    [Fact]
    public async Task DefineRememberCommandHandler()
    {
        //Arrange
        var expectedTerm = "Sambarril";
        //User? callbackUser = default;
        var contextMock = new Mock<BotContext>();
        var acessor = new Mock<ISearchAccessor>();

        var user = new User(TestHelper.AdminId, "Rogim Nazario", "pt-br", false);
        var search = new Search(expectedTerm, user);
        acessor.Setup(s => s.CurrentSearch()).Returns(search);
        var command = new DefineRememberCommand();
        var handler = new DefineRememberCommandHandler(new Mock<ITelegramSender>().Object, contextMock.Object, new Mock<IAdminConfiguration>().Object,
            new Mock<IUnitOfWork>().Object, acessor.Object);

        //Act
        await handler.Handle(command, CancellationToken.None);

        //Assert
        contextMock.Verify(c => c.Update(It.Is<User>(u => VerifyUser(u, user, expectedTerm))));
    }

    private bool VerifyUser(User currentUser, User expectedUser, string expectedTerm)
    {
        currentUser.telegramid.Should().Be(expectedUser.telegramid);
        currentUser.Should().NotBeNull();
        currentUser!.searchesscheduled.Should().Be($"{expectedTerm}|{DateTime.Now:dd/MM/yyyy}");

        return true;
    }
}