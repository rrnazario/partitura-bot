using HtmlAgilityPack;
using System.Text.RegularExpressions;
using TelegramPartHook.Application.Helpers;
using TelegramPartHook.Application.Interfaces.Searches;
using TelegramPartHook.Domain.SeedWork;
using TelegramPartHook.Infrastructure.Helpers;
using static TelegramPartHook.Domain.Constants.Enums;

namespace TelegramPartHook.Application.Services.Searches.Crawlers
{
    public class BlogspotCrawlerSearchService
       : IBlogspotCrawlerSearchService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogHelper _log;

        public BlogspotCrawlerSearchService(IHttpClientFactory httpClientFactory,
                                            ILogHelper log)
        {

            _httpClient = httpClientFactory.CreateClient();
            _log = log;
        }

        public async Task<IEnumerable<SheetSearchResult>> SearchAsync(string term, CancellationToken cancellationToken)
        {
            var sites = new List<string>()
            {
                "https://meucavaquinhopartituras.blogspot.com.br/search?q=",
                "https://partiturasdesambaepagode.blogspot.com.br/search/?q="
            };

            var result = new List<SheetSearchResult>();

            foreach (var site in sites)
            {
                try
                {
                    var httpResult = await _httpClient.GetAsync(new Uri(string.Concat(site, term.AdjustSearch())), cancellationToken);

                    string htmlCode = await httpResult.Content.ReadAsStringAsync(cancellationToken);

                    if (htmlCode.Contains("nenhuma postagem correspondente para a consulta", StringComparison.InvariantCultureIgnoreCase))
                        continue;

                    var pattern = new Regex("<a href=\'(.*?)\'>");
                    var matches = pattern.Matches(htmlCode);

                    if (matches.Count == 0)
                        continue;

                    var tasks = new List<Task<List<SheetSearchResult>>>();
                    foreach (Match match in matches)
                    {
                        tasks.Add(Task.Run(async () =>
                        {
                            var innerResult = new List<SheetSearchResult>();

                            foreach (Group group in match.Groups)
                            {
                                if (group.Value.StartsWith("http") && group.Value.ToLowerInvariant().Contains(string.Join("-", term.Split(' ')).ToLowerInvariant().ReplaceDiacritics()))
                                {
                                    var newUrl = group.Value;

                                    try
                                    {
                                        httpResult = await _httpClient.GetAsync(newUrl, cancellationToken);
                                        if (httpResult.IsSuccessStatusCode)
                                            htmlCode = await httpResult.Content.ReadAsStringAsync(cancellationToken);
                                        else
                                            continue;
                                    }
                                    catch { continue; }

                                    var html = new HtmlDocument();
                                    html.LoadHtml(htmlCode);

                                    var imagesUrl = html.DocumentNode.SelectNodes("//div[@class='separator']");
                                    if (imagesUrl != null && imagesUrl.Any())
                                        foreach (var linkImagem in imagesUrl)
                                        {
                                            var imgUrl = linkImagem.ChildNodes["a"]?.Attributes["href"]?.Value;
                                            if (!string.IsNullOrEmpty(imgUrl) && !result.Any(r => r.Address.EndsWith(imgUrl)))
                                                innerResult.Add(new(imgUrl.StartsWith("http", StringComparison.InvariantCultureIgnoreCase) ? imgUrl : $"http:{imgUrl}", FileSource.Crawler));
                                        }
                                }
                            }

                            return innerResult;
                        }));
                    }

                    await Task.WhenAll(tasks);

                    result.AddRange(tasks.SelectMany(m => m.Result));

                }
                catch (Exception e)
                {
                    _log.Info(e, cancellationToken);
                }
            };

            return result;
        }
    }
}