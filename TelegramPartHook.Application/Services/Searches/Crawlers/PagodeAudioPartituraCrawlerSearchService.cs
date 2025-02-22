using HtmlAgilityPack;
using TelegramPartHook.Application.Helpers;
using TelegramPartHook.Application.Interfaces.Searches;
using TelegramPartHook.Domain.SeedWork;
using static TelegramPartHook.Domain.Constants.Enums;

namespace TelegramPartHook.Application.Services.Searches.Crawlers
{
    public class PagodeAudioPartituraCrawlerSearchService
       : IPagodeAudioPartituraCrawlerSearchService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogHelper _log;

        public PagodeAudioPartituraCrawlerSearchService(IHttpClientFactory httpClientFactory,
                                            ILogHelper log)
        {

            _httpClient = httpClientFactory.CreateClient();
            _log = log;
        }

        public async Task<IEnumerable<SheetSearchResult>> SearchAsync(string term, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(term))
                throw new ArgumentException("termo");

            var returnImages = new List<SheetSearchResult>();

            var result = await _httpClient.GetAsync(new Uri(Path.Combine("http://pagodeaudioepartitura.blogspot.com/search/?q=", term)));

            var html = new HtmlDocument();
            html.LoadHtml(await result.Content.ReadAsStringAsync());
            var nosPesquisaPartitura = html.DocumentNode.SelectNodes("//h3[@class='post-title entry-title']");

            var tasks = new List<Task<List<SheetSearchResult>>>();
            if (nosPesquisaPartitura != null)
                foreach (var ahref in nosPesquisaPartitura)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        var innerItems = new List<SheetSearchResult>();
                        try
                        {
                            var linkPaginaPartitura = ahref.InnerHtml.Replace("\n", "").Trim().Split('=')[1].Split('>')[0].Replace("'", "");

                            result = await _httpClient.GetAsync(linkPaginaPartitura);

                            html.LoadHtml(result.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                            var linkImagens = html.DocumentNode.SelectNodes("//div[@class='separator']");

                            foreach (var linkImagem in linkImagens)
                            {
                                var imgUrl = linkImagem.ChildNodes["a"]?.Attributes["href"]?.Value;
                                if (!string.IsNullOrEmpty(imgUrl))
                                    innerItems.Add(new(imgUrl, FileSource.Crawler));
                            }

                        }
                        catch { }

                        return innerItems;
                    }));
                }

            await Task.WhenAll(tasks);

            returnImages.AddRange(tasks.SelectMany(m => m.Result));

            return returnImages.Distinct();
        }
    }
}