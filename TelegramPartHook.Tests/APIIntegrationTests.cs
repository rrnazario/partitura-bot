using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using TelegramPartHook.Application.Interfaces.Searches;
using System.Threading;
using FluentAssertions;
using TelegramPartHook.Tests.Core.Fixtures;

namespace TelegramPartHook.UnitTests;

public class APIIntegrationTests : IClassFixture<CoreDependencyInjectionFixture>
{
    private readonly ServiceProvider _serviceProvider;

    public APIIntegrationTests(CoreDependencyInjectionFixture fixture)
    {
        _serviceProvider = fixture.ServiceProvider;

        //EnvironmentHelper.DefineEvironmentVariables();
    }

    [Fact]
    public async Task GetDropboxImages()
    {
        var service = _serviceProvider.GetService<IDropboxService>();

        var result = await service.SearchAsync("dizer por dizer", CancellationToken.None);

        result.Should().NotBeEmpty();
    }

    [InlineData("fur elise", 7)]
    [InlineData("Seguindo Viagem", 0)] //no results
    [InlineData("Lembrança belo", 0)] //no results
    [InlineData("Mais só do que sozinho", 0)] //no results
    [Theory]
    public async Task GetPdfChoroCantoriumFiles(string term, int total)
    {
        var service = _serviceProvider.GetService<IChoroSearchService>();

        var result = await service.SearchChorosOnCantoriumPdfAsync(term);

        result.Count().Should().Be(total);
    }

    [InlineData("noel rosa", 16)]
    [InlineData("Sambarril", 0)]
    [Theory]
    public async Task GetPdfChoroCasaDoChoroFiles(string term, int total)
    {
        var service = _serviceProvider.GetService<IChoroSearchService>();

        var result = await service.SearchChorosOnCasaDoChoroPdfAsync(term);

        result.Count().Should().Be(total);
    }

    [Fact]
    public async Task ExtractPagodeAudioPartituraResults()
    {
        var service = _serviceProvider.GetService<IPagodeAudioPartituraCrawlerSearchService>();

        var result = await service.SearchAsync("open house", CancellationToken.None);

        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ExtractNandinhoCavacoByLabelResults()
    {
        var service = _serviceProvider.GetService<INandinhoCrawlerSearchService>();

        var result = await service.SearchAsync("open bar do Caju", CancellationToken.None);

        result.Should().NotBeEmpty();
    }

    [Fact(Skip = "Flacky test, depends on 3rd party blocks.")]
    public async Task ExtractNandinhoCavacoResults()
    {
        var service = _serviceProvider.GetService<INandinhoCrawlerSearchService>();

        var result = await service.SearchAsync("sinto sua falta", CancellationToken.None);

        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ExtractBlogspotSheetsResults()
    {
        var service = _serviceProvider.GetService<IBlogspotCrawlerSearchService>();

        var result = await service.SearchAsync("sinto sua falta", CancellationToken.None);

        result.Should().NotBeEmpty();
    }
}