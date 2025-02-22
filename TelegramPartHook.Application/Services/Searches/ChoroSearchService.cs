using HtmlAgilityPack;
using TelegramPartHook.Application.Interfaces.Searches;
using TelegramPartHook.Domain.SeedWork;
using TelegramPartHook.Infrastructure.Helpers;
using static TelegramPartHook.Domain.Constants.Enums;

namespace TelegramPartHook.Application.Services.Searches
{
    public class ChoroSearchService : IChoroSearchService
    {
        private readonly HttpClient _httpClient;

        public ChoroSearchService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task<IEnumerable<SheetSearchResult>> SearchAsync(string term, CancellationToken cancellationToken)
        {
            var returnFiles = new List<SheetSearchResult>();

            try
            {
                returnFiles.AddRange(await SearchChorosOnCasaDoChoroPdfAsync(term));
                returnFiles.AddRange(await SearchChorosOnCantoriumPdfAsync(term));
            }
            catch { }

            return returnFiles;
        }

        public async Task<IEnumerable<SheetSearchResult>> SearchChorosOnCasaDoChoroPdfAsync(string term)
        {
            var returnFiles = new List<SheetSearchResult>();

            try
            {
                var baseUrl = @"http://acervo.casadochoro.com.br";

                var result = await _httpClient.GetAsync($@"{baseUrl}/Works/index?title={term.AdjustSearch()}");

                var html = new HtmlDocument();
                html.LoadHtml(await result.Content.ReadAsStringAsync());

                var firstPageLinks = html.DocumentNode.SelectNodes("//a[starts-with(@href, '/works/view')]")?.Select(s => s.Attributes["href"].Value);

                if (firstPageLinks == null)
                {
                    result = await _httpClient.GetAsync($@"{baseUrl}/Works/index?title=&date_start=&date_end=&artistic_name={term.AdjustSearch()}&arranger=&genre=");
                    html.LoadHtml(await result.Content.ReadAsStringAsync());

                    firstPageLinks = html.DocumentNode.SelectNodes("//a[starts-with(@href, '/works/view')]")?.Select(s => s.Attributes["href"].Value);
                }

                var tasks = new List<Task<List<SheetSearchResult>>>();

                if (firstPageLinks != null)
                {
                    foreach (var itemLista in firstPageLinks)
                    {
                        tasks.Add(Task.Run(async () =>
                        {
                            var innerResult = new List<SheetSearchResult>();
                            var link = $"{baseUrl}{itemLista}";
                            if (!Uri.TryCreate(link, UriKind.RelativeOrAbsolute, out var uri)) return innerResult;

                            //vai pra proxima pagina pra extrair as URLs das partituras
                            var html = new HtmlDocument();

                            var paginaViewPartitura = await _httpClient.GetAsync(link);
                            html.LoadHtml(await paginaViewPartitura.Content.ReadAsStringAsync());

                            var itensPdf = html.DocumentNode.SelectNodes("//a[@class='dwl_score sprite-small_icons sprite-small_scores']")?.Select(s => $"{baseUrl}{s.Attributes["href"]?.Value}");

                            if (itensPdf != null && itensPdf.Any())
                                innerResult.AddRange(itensPdf.Select(s => new SheetSearchResult(s, FileSource.Crawler)));

                            return innerResult;
                        }));
                    }

                    await Task.WhenAll(tasks);

                    returnFiles.AddRange(tasks.SelectMany(m => m.Result));
                }
            }
            catch { }

            return returnFiles;
        }

        public async Task<IEnumerable<SheetSearchResult>> SearchChorosOnCantoriumPdfAsync(string term)
        {
            var returnFiles = new List<SheetSearchResult>();

            try
            {
                var baseUrl = @"http://pt.cantorion.org/";

                var result = await _httpClient.GetAsync($@"{baseUrl}musicsearch?form_basic__action=&q={term.AdjustSearch()}");
                var html = new HtmlDocument();
                var content = await result.Content.ReadAsStringAsync();

                if (content.Contains("Nenhuma obra encontrada."))
                    return returnFiles;

                html.LoadHtml(content);

                var firstPageLinksT = html.DocumentNode.SelectNodes("//a[@class='thumbnailLink']")?.Select(s => s.Attributes["href"].Value);
                var firstPageLinks = html.DocumentNode.SelectNodes("//span[@dir='ltr']")?
                                         .Select(s => new { url = s.ParentNode?.Attributes["href"]?.Value ?? "", text = s.InnerText });

                firstPageLinks = firstPageLinks.Where(w => !string.IsNullOrEmpty(w.url) && w.text.ReplaceDiacritics().Contains(term.ReplaceDiacritics(), StringComparison.InvariantCultureIgnoreCase))
                                 .ToList();

                var tasks = new List<Task<List<SheetSearchResult>>>();

                if (firstPageLinks != null && firstPageLinks.Any())
                {
                    foreach (var itemLista in firstPageLinks)
                    {
                        tasks.Add(Task.Run(async () =>
                        {
                            var innerResult = new List<SheetSearchResult>();
                            var html = new HtmlDocument();

                            var link = $"{baseUrl}{itemLista.url}";
                            if (!Uri.TryCreate(link, UriKind.RelativeOrAbsolute, out var uri)) return innerResult;

                            //vai pra proxima pagina pra extrair as URLs das partituras
                            var paginaViewPartitura = await _httpClient.GetAsync(link);
                            html.LoadHtml(await paginaViewPartitura.Content.ReadAsStringAsync());

                            var itensPdf = html.DocumentNode.SelectNodes("//a[@class='track button cdn_href downloadLink']")?.Select(s => s.Attributes["href"]?.Value);

                            if (itensPdf != null && itensPdf.Any())
                                returnFiles.AddRange(itensPdf.Select(s => new SheetSearchResult(s, FileSource.Crawler)));

                            return innerResult;
                        }));
                    }
                }

                await Task.WhenAll(tasks);

                returnFiles.AddRange(tasks.SelectMany(m => m.Result));                
            }
            catch { }

            return returnFiles.Distinct();
        }
    }
}
