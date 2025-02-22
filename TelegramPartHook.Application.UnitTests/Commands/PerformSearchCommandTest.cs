using TelegramPartHook.Application.Factories;
using TelegramPartHook.Application.Queries;
using TelegramPartHook.Application.Services;
using TelegramPartHook.Domain.SeedWork;
using TelegramPartHook.Infrastructure.Persistence;
using TelegramPartHook.Tests.Core.Helpers;

namespace TelegramPartHook.Application.UnitTests.Commands
{
    public class PerformSearchCommandTest
    {
        [Fact]
        public async Task PerformSearchCommandHandler()
        {
            //Arrange
            var expectedTerm = "Sambarril";
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var contextMock = new Mock<BotContext>();
            var acessor = new Mock<ISearchAccessor>();

            var mediatorMock = new Mock<MediatR.IMediator>();
            mediatorMock.Setup(_ => _.Send(It.IsAny<GetSheetLinksQuery>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<IEnumerable<SheetSearchResult>>([new("someUrl.com.br", Enums.FileSource.Crawler)]));

            var search = new Search(expectedTerm, new(TestHelper.AdminId, "Rogim Nazario", "pt-br", false));
            acessor.Setup(s => s.CurrentSearch()).Returns(search);
            var command = new PerformSearchCommand();

            var handler = new PerformSearchCommandHandler(
                new Mock<IGlobalState>().Object,
                new Mock<ITelegramSender>().Object,
                mediatorMock.Object,
                new Mock<ILogHelper>().Object,
                contextMock.Object,
                unitOfWorkMock.Object,
                acessor.Object);

            //Act
            await handler.Handle(command, CancellationToken.None);

            //Assert
            contextMock.Verify(c => c.Update(It.Is<User>(u => u.searchesok == expectedTerm)));
        }
    }
}
