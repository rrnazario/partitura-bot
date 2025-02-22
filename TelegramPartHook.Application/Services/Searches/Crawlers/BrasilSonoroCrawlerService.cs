using System.Web;
using HtmlAgilityPack;
using TelegramPartHook.Application.Helpers;
using TelegramPartHook.Application.Interfaces.Searches;
using TelegramPartHook.Domain.Constants;
using TelegramPartHook.Domain.SeedWork;
using TelegramPartHook.Infrastructure.Helpers;

namespace TelegramPartHook.Application.Services.Searches.Crawlers;

public class BrasilSonoroCrawlerService
    : IBrasilSonoroCrawlerService
{
    private readonly HttpClient _httpClient;
    private readonly ILogHelper _log;

    public BrasilSonoroCrawlerService(ILogHelper log, HttpClient httpClient)
    {
        _log = log;
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<SheetSearchResult>> SearchAsync(string term, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(term))
                throw new ArgumentException("termo");

            var returnImages = new List<SheetSearchResult>();

            var uri = $"https://brasilsonoro.com/?s={HttpUtility.UrlEncode(term.AdjustSearch())}";

            var result =
                await _httpClient.GetAsync(new Uri(uri), HttpCompletionOption.ResponseContentRead, cancellationToken);
            var html = new HtmlDocument();
            html.LoadHtml(await result.Content.ReadAsStringAsync(cancellationToken));
            var nosPesquisaPartitura = html.DocumentNode.SelectNodes("//h3[@class='entry-title td-module-title']")?
                .Where(w => !string.IsNullOrEmpty(w.InnerText) &&
                            (w.InnerText.Contains(term, StringComparison.InvariantCultureIgnoreCase) || term.Split(' ')
                                .Any(t => w.InnerText.Contains(t, StringComparison.InvariantCultureIgnoreCase))));

            var tasks = new List<Task<List<SheetSearchResult>>>();

            foreach (var htmlNode in nosPesquisaPartitura ?? Array.Empty<HtmlNode>())
            {
                tasks.Add(Task.Run(async () =>
                {
                    var listResult = new List<SheetSearchResult>();

                    try
                    {
                        var linkPaginaPartitura = htmlNode.ChildNodes["a"].Attributes["href"].Value;
                        var title = htmlNode.ChildNodes["a"].Attributes["title"].Value;

                        result = await _httpClient.GetAsync(linkPaginaPartitura, cancellationToken);

                        html.LoadHtml(await result.Content.ReadAsStringAsync(cancellationToken));
                        var linkImagens = html.DocumentNode.SelectNodes("//div[@id='caixa-down']");

                        foreach (var linkImagem in linkImagens)
                        {
                            var pdfUrl = linkImagem.ChildNodes["a"]?.Attributes["href"].Value;
                            if (!string.IsNullOrEmpty(pdfUrl))
                            {
                                var rnd = new Random(DateTime.Now.Millisecond);

                                listResult.Add(new(pdfUrl, Enums.FileSource.CrawlerDownloadLink,
                                    additionalInfo:
                                    $"{title} {DateTime.Now.AddMilliseconds(rnd.Next() + listResult.Count + 1):fffffff}"));
                            }
                        }
                    }
                    catch
                    {
                    }

                    return listResult;
                }, cancellationToken));
            }

            await Task.WhenAll(tasks);

            returnImages.AddRange(tasks.SelectMany(s => s.Result));

            return returnImages.Distinct();
        }
        catch (Exception e)
        {
            _log.Info(e, cancellationToken);
            return Array.Empty<SheetSearchResult>();
        }
    }
}