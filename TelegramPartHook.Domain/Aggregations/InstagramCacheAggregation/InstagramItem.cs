using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace TelegramPartHook.Domain.Aggregations.InstagramCacheAggregation
{
    public class InstagramItem
    {
        public string Text { get; private set; }
        public List<string> ImageUrls { get; private set; }

        [JsonConstructor]
        public InstagramItem(string Text, List<string> ImageUrls)
        {
            this.Text = Text;
            this.ImageUrls = ImageUrls;
        }

        public InstagramItem(JObject obj)
        {
            ImageUrls = new();

            Text = (string)obj["node"]["edge_media_to_caption"]["edges"][0]["node"]["text"];

            try
            {
                var images = obj["node"]["edge_sidecar_to_children"]["edges"].Value<JArray>();

                foreach (JObject image in images)
                    ImageUrls.Add((string)image["node"]["display_url"]);
            }
            catch //Pagination (so far only getting first page)
            {
                var displayUrl = (string)obj["node"]["display_url"];
                ImageUrls.Add(Regex.Unescape(displayUrl));
            }
        }
    }
}
