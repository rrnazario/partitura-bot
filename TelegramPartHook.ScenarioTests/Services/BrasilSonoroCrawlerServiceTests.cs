using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TelegramPartHook.Application.Helpers;
using TelegramPartHook.Application.Services.Searches.Crawlers;
using TelegramPartHook.Tests.Core.Fixtures;
using Xunit;

namespace TelegramPartHook.ComponentTests.Services;

public class BrasilSonoroCrawlerServiceTests
    :IClassFixture<CoreDependencyInjectionFixture>
{
    private readonly ServiceProvider _serviceProvider;

    public BrasilSonoroCrawlerServiceTests(CoreDependencyInjectionFixture fixture)
    {
        _serviceProvider = fixture.ServiceProvider;
    }
    
    [Fact]
    public async Task BrasilSonoroCrawlerService_ShouldReturnProperly()
    {
        var target = new BrasilSonoroCrawlerService(
            new Mock<ILogHelper>().Object,
            _serviceProvider.GetService<HttpClient>()!);

        var result = await target.SearchAsync("Lauana Prado", CancellationToken.None);

        result.Should().NotBeEmpty();
    }
}