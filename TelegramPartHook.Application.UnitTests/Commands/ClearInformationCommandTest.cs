using Microsoft.Extensions.DependencyInjection;
using TelegramPartHook.Application.Commands.ClearInformation;
using TelegramPartHook.Tests.Core.Helpers;

namespace TelegramPartHook.Application.UnitTests.Commands;

public class ClearInformationCommandTest
{
    [Fact]
    public void ClearInformationCommandHandler()
    {
        var factory = new ClearInformationFactory(new Mock<IServiceScopeFactory>().Object);

        var search = new Search("/clear ok", TestHelper.AdminUser);
        var result = factory.Create(search);

        result.Should().BeOfType<ClearDatabaseInformation>();
        (result as ClearDatabaseInformation)!.Query.Should().Be("UPDATE client SET searchesok = NULL");
    }
}