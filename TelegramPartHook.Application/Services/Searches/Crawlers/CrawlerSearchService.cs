using TelegramPartHook.Application.Interfaces.Searches;
using TelegramPartHook.Domain.SeedWork;

namespace TelegramPartHook.Application.Services.Searches.Crawlers;

public class CrawlerSearchService
    : ICrawlerSearchService
{
    private readonly IPagodeAudioPartituraCrawlerSearchService _pagodeAudioPartituraCrawler;
    private readonly IEnumerable<ISearchService> _crawlers;

    public CrawlerSearchService(IEnumerable<ISearchService> crawlers,
        IPagodeAudioPartituraCrawlerSearchService pagodeAudioPartituraCrawler,
        IBrasilSonoroCrawlerService brasilSonoroCrawlerService)
    {
        _crawlers = crawlers.Concat([brasilSonoroCrawlerService]);

        _pagodeAudioPartituraCrawler = pagodeAudioPartituraCrawler;
    }

    public Task<IEnumerable<SheetSearchResult>> SearchAsync(string term, CancellationToken cancellationToken)
    {
        var results = new List<SheetSearchResult>();

        results.AddRange(_pagodeAudioPartituraCrawler.SearchAsync(term, cancellationToken).GetAwaiter()
            .GetResult());

        if (!results.Any())
        {
            foreach (var crawler in _crawlers)
            {
                results.AddRange(crawler.SearchAsync(term, cancellationToken).GetAwaiter().GetResult());
            }
        }

        return Task.FromResult(results.AsEnumerable());
    }
}