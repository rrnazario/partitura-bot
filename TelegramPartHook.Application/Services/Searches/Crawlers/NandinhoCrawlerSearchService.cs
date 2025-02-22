using HtmlAgilityPack;
using TelegramPartHook.Application.Helpers;
using TelegramPartHook.Application.Interfaces.Searches;
using TelegramPartHook.Domain.SeedWork;
using TelegramPartHook.Infrastructure.Helpers;
using static TelegramPartHook.Domain.Constants.Enums;

namespace TelegramPartHook.Application.Services.Searches.Crawlers
{
    public class NandinhoCrawlerSearchService
       : INandinhoCrawlerSearchService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogHelper _log;

        public NandinhoCrawlerSearchService(IHttpClientFactory httpClientFactory,
                                            ILogHelper log)
        {

            _httpClient = httpClientFactory.CreateClient();
            _log = log;
        }

        public async Task<IEnumerable<SheetSearchResult>> SearchAsync(string term, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrEmpty(term))
                    throw new ArgumentException("termo");

                var returnImages = new List<SheetSearchResult>();

                var result = await _httpClient.GetAsync(new Uri(Path.Combine("https://www.nandinhocavaco.com.br/search?q=", term.AdjustSearch())));

                var html = new HtmlDocument();
                html.LoadHtml(await result.Content.ReadAsStringAsync());
                var nosPesquisaPartitura = html.DocumentNode.SelectNodes("//h3[@class='post-title entry-title']")?
                                                            .Where(w => !string.IsNullOrEmpty(w.InnerText) && (w.InnerText.Contains(term, StringComparison.InvariantCultureIgnoreCase) || term.Split(' ').Any(t => w.InnerText.Contains(t, StringComparison.InvariantCultureIgnoreCase))));

                if (nosPesquisaPartitura == null)
                {
                    result = await _httpClient.GetAsync(new Uri($"https://www.nandinhocavaco.com.br/search/label/{term.AdjustSearchLabel()}"));

                    html.LoadHtml(await result.Content.ReadAsStringAsync());
                    nosPesquisaPartitura = html.DocumentNode.SelectNodes("//h3[@class='post-title entry-title']")?.Where(w => !string.IsNullOrEmpty(w.InnerText));
                }

                var tasks = new List<Task<List<SheetSearchResult>>>();

                foreach (var no in nosPesquisaPartitura ?? Array.Empty<HtmlNode>())
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        var listResult = new List<SheetSearchResult>();

                        try
                        {
                            var linkPaginaPartitura = no.ChildNodes["a"].Attributes["href"].Value;

                            result = await _httpClient.GetAsync(linkPaginaPartitura);

                            html.LoadHtml(await result.Content.ReadAsStringAsync());
                            var linkImagens = html.DocumentNode.SelectNodes("//div[@class='separator']");

                            foreach (var linkImagem in linkImagens)
                            {
                                var imgUrl = linkImagem.ChildNodes["img"]?.Attributes["src"].Value ?? linkImagem.ChildNodes["a"]?.ChildNodes["img"]?.Attributes["src"].Value;
                                if (!string.IsNullOrEmpty(imgUrl))
                                    listResult.Add(new(imgUrl.StartsWith("http", StringComparison.InvariantCultureIgnoreCase) ? imgUrl : $"http:{imgUrl}", FileSource.Crawler));
                            }
                        }
                        catch { }

                        return listResult;
                    }));
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
}