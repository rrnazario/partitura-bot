using FluentAssertions;
using TelegramPartHook.Domain.Aggregations.UserAggregation;
using TelegramPartHook.Domain.Helpers;
using TelegramPartHook.Tests.Core.Helpers;

namespace TelegramPartHook.Domain.UnitTests.Helpers;

public class PixHelperTests : IDisposable
{
    private string pixFile = string.Empty;
    private User userStub = new User(TestHelper.AdminId, "Rogim Nazario", "pt-br", false);

    [Fact]
    public void GeneratingPixStringProperly()
    {
        //Act / Assert
        var pix = PixHelper.GeneratePixString(userStub);

        pix.Should().NotBeEmpty();
    }

    [Fact]
    public void GeneratingPixImageProperly()
    {
        //Act / Assert
        pixFile = PixHelper.GenerateQrCodeImage(userStub);

        pixFile.Should().NotBeEmpty();
        File.Exists(pixFile).Should().BeTrue();
    }

    public void Dispose()
    {
        if (File.Exists(pixFile))
            File.Delete(pixFile);
    }
}