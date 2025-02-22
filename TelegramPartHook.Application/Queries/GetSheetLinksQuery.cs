using MediatR;
using TelegramPartHook.Application.Helpers;
using TelegramPartHook.Application.Interfaces;
using TelegramPartHook.Application.Interfaces.Searches;
using TelegramPartHook.Domain.SeedWork;
using TelegramPartHook.Infrastructure.Helpers;

namespace TelegramPartHook.Application.Queries;

public record GetSheetLinksQuery(string Term, bool UseInstagram = true)
    : IRequest<IEnumerable<SheetSearchResult>>;

public class GetSheetLinksQueryHandler
    : IRequestHandler<GetSheetLinksQuery, IEnumerable<SheetSearchResult>>
{
    private readonly ILogHelper _log;

    private readonly IChoroSearchService _choroSearchService;
    private readonly ICacheSearchService _cacheSearchService;
    private readonly IDropboxService _dropboxSearchService;
    private readonly ICrawlerSearchService _crawlerSearchService;
    private readonly IInstagramCrawlerSearchService _instaService;
    private readonly ISanitizeService _sanitizeService;

    private static readonly string[] ObviousReplaceTerms =
    [
        "partituras",
        "partitura",
        "cifras",
        "cifra"
    ];

    public GetSheetLinksQueryHandler(ILogHelper log,
        IChoroSearchService choroSearchService,
        IDropboxService dropboxSearchService,
        ICrawlerSearchService crawlerSearchService,
        ICacheSearchService cacheSearchService,
        IInstagramCrawlerSearchService instaService,
        ISanitizeService sanitizeService)
    {
        _log = log;
        _choroSearchService = choroSearchService;
        _dropboxSearchService = dropboxSearchService;
        _crawlerSearchService = crawlerSearchService;
        _cacheSearchService = cacheSearchService;
        _instaService = instaService;
        _sanitizeService = sanitizeService;
    }

    public async Task<IEnumerable<SheetSearchResult>> Handle(GetSheetLinksQuery request,
        CancellationToken cancellationToken)
    {
        var term = request.Term.RemoveMultipleSpaces().Trim();

        if (string.IsNullOrEmpty(term))
            return [];

        term = TryFixSearchTerm(term);

        var sheetSearchResults = (await _cacheSearchService.SearchAsync(term, cancellationToken)).ToList();
        if (sheetSearchResults.Any())
            return await _sanitizeService.TrySanitizeResultsAsync(sheetSearchResults);

        int attempt = 0, maxAttempts = 2;

        do
        {
            _log.Info($"Attempt {attempt + 1}, Term: '{term}'", cancellationToken);

            if (term.EndsWith("choro", StringComparison.InvariantCultureIgnoreCase)) //Forçar só pesquisa de choro
            {
                //term = term.Substring(0, term.Length - "choro".Length).Trim();
                term = term[..^"choro".Length].Trim();

                _log.Info("PESQUISANDO SOMENTE CHOROS", cancellationToken);
                sheetSearchResults.AddRange(await _choroSearchService.SearchAsync(term, cancellationToken)
                    .ConfigureAwait(false));
            }
            else
            {
                _log.Info("PESQUISANDO CRAWLERS", cancellationToken);
                sheetSearchResults.AddRange(
                    await _crawlerSearchService.SearchAsync(term, cancellationToken).ConfigureAwait(false));

                try
                {
                    _log.Info("PESQUISANDO DROPBOX", cancellationToken);
                    sheetSearchResults.AddRange(_dropboxSearchService.SearchAsync(term, cancellationToken).GetAwaiter()
                        .GetResult());
                }
                catch (Exception e)
                {
                    _log.Info($"ERRO PESQUISANDO DROPBOX: {e.Message}\n\n{e.StackTrace}", cancellationToken, true);
                }

                if (sheetSearchResults.Count == 0) //buscar PDF
                {
                    _log.Info($"PESQUISANDO CHOROS", cancellationToken);
                    sheetSearchResults.AddRange(await _choroSearchService.SearchAsync(term, cancellationToken)
                        .ConfigureAwait(false));
                }

                if (sheetSearchResults.Count == 0 && request.UseInstagram)
                {
                    _log.Info($"PESQUISANDO NO INSTAGRAM", cancellationToken);
                    sheetSearchResults.AddRange(await _instaService.SearchAsync(term, cancellationToken)
                        .ConfigureAwait(false));
                }
            }

            if (sheetSearchResults.Count == 0 && term.ContainsDiacritics())
            {
                attempt++;
                term = term.ReplaceDiacritics();
            }
            else break;
        } while (attempt < maxAttempts);

        if (sheetSearchResults.Count == 0)
        {
            return sheetSearchResults;
        }

        sheetSearchResults = await _sanitizeService.TrySanitizeResultsAsync(sheetSearchResults);

        await _cacheSearchService.SaveResultsAsync(term, sheetSearchResults.ToArray(), cancellationToken);

        return sheetSearchResults;
    }

    private static string TryFixSearchTerm(string originalTerm)
    {
        var result = TryPerformObviousChanges(originalTerm);

        return ObviousReplaceTerms.Aggregate(result,
            (current, term) => current.Replace(term, string.Empty, StringComparison.InvariantCultureIgnoreCase));
    }

    private static string TryPerformObviousChanges(string originalTerm)
    {
        var result = FixingTerms.Where(p =>
                p.Value.Any(t => t.Equals(originalTerm, StringComparison.InvariantCultureIgnoreCase)))
            .ToArray();

        return result.Length != 0 ? result.FirstOrDefault().Key : originalTerm;
    }

    private static readonly Dictionary<string, string[]> FixingTerms = new()
    {
        { "marvvila", new[] { "marvila", "marvilla" } }
    };
}