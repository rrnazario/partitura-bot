using System.Net.Http;
using Newtonsoft.Json.Linq;
using Serilog;
using TelegramPartHook.Domain.Constants;

namespace TelegramPartHook.Domain.Aggregations.InstagramCacheAggregation
{
    public interface IInstaExtractor
    {
        string BaseNode { get; }
        string Url { get; }
    }

    public abstract class InstaBaseExtractor : IInstaExtractor
    {
        protected readonly JObject parsedContent;
        private readonly IAdminConfiguration _adminConfiguration;

        public abstract string BaseNode { get; }
        public abstract string Url { get; }

        public bool IsValid { get; private set; } = true;

        public string DefineUrl(bool useProxy)
            => useProxy ? DefineProxyUrl(Url) : Url;

        protected string DefineProxyUrl(string originalUrl)
            => @$"https://api.webscraping.ai/html?api_key={_adminConfiguration.WebScrapingToken}&url={Uri.EscapeDataString(originalUrl)}&js=false&proxy=residential";

        public InstaBaseExtractor(HttpClient _client, IAdminConfiguration adminConfiguration)
        {
            _adminConfiguration = adminConfiguration;

            int currentAttempt = 1, maxAttempt = 2;
            bool useProxy = false;
            do
            {
                try
                {
                    parsedContent = JObject.Parse(GetContentAsync(_client, useProxy).GetAwaiter().GetResult());
                    return;
                }
                catch (Exception e)
                {
                    Log.Logger.Information($"Error in {nameof(InstaBaseExtractor)}: {e.Message}\n\n{e.StackTrace}");
                    useProxy = true;
                    currentAttempt++;
                }

            }
            while (currentAttempt <= maxAttempt);

            IsValid = false;
        }

        public List<InstagramItem> ExtractInstagramItems()
        {
            var edgesArray = parsedContent[BaseNode]["user"]["edge_owner_to_timeline_media"]["edges"].Value<JArray>();

            var result = new List<InstagramItem>();

            foreach (JObject item in edgesArray)
                result.Add(new(item));

            return result;
        }

        public string ExtractEndCursor()
            => (string)parsedContent[BaseNode]["user"]["edge_owner_to_timeline_media"]["page_info"]["end_cursor"];

        protected virtual async Task<string> GetContentAsync(HttpClient _httpClient, bool useProxy)
        {
            var url = DefineUrl(useProxy);

            var result = await _httpClient.GetAsync(new Uri(url));
            var content = await result.Content.ReadAsStringAsync(); ;

            Log.Logger.Information($"[{nameof(GetContentAsync)}] \n {content.Substring(0, 30)}");

            if (!IsInstagramValidJson(content))
                throw new InvalidOperationException("Invalid JSON");

            return content;
        }

        private bool IsInstagramValidJson(string input)
            => IsValidJson(input) && input.StartsWith("{\"seo_category_infos\"");

        private bool IsValidJson(string input)
        {
            try
            {
                var x = JToken.Parse(input);
                return x is not null;
            }
            catch
            {
                return false;
            }
        }
    }

    public class InstaInitialExtractor : InstaBaseExtractor
    {
        public override string BaseNode => "graphql";

        public override string Url => @"https://www.instagram.com/linhas.espacos/?__a=1&__d=1";

        public InstaInitialExtractor(HttpClient client, IAdminConfiguration adminConfiguration) : base(client, adminConfiguration) { }

        public long ExtractGraphqlId()
            => (long)parsedContent[BaseNode]["user"]["id"];

        public string ExtractUserName()
            => (string)parsedContent[BaseNode]["user"]["username"];

        // protected override Task<string> GetContentAsync(HttpClient _httpClient, bool useProxy)
        //     => Task.FromResult(
        //         File.ReadAllText(@"C:\Repos\telegramparthook\TelegramPartHook.ScenarioTests\Files\instagramSample.json")
        //     );
    }

    public class InstaPaginatedExtractor : InstaBaseExtractor
    {
        public override string BaseNode => "data";

        public override string Url
        => @$"https://www.instagram.com/graphql/query/?query_id=17888483320059182&id={_graphqlId}&first=48&after={ExtractEndCursor()}";

        private long _graphqlId;

        public InstaPaginatedExtractor(HttpClient _client, InstaInitialExtractor initialExtractor, IAdminConfiguration adminConfiguration)
        : base(_client, adminConfiguration)
            => _graphqlId = initialExtractor.ExtractGraphqlId();

        // protected override Task<string> GetContentAsync(HttpClient _httpClient, bool useProxy)
        //     => Task.FromResult(
        //         File.ReadAllText(@"C:\Repos\telegramparthook\TelegramPartHook.ScenarioTests\Files\instagramPaginationSample.json")
        //     );
    }
}