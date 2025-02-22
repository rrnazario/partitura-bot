using System;
using System.Net.Http;
using System.Threading.Tasks;
using TelegramPartHook.Domain.Aggregations.InstagramCacheAggregation;

namespace TelegramPartHook.Infrastructure.Helpers.Instagram
{

    public static class InstagramCacheExtensions
    {
        private static Lazy<HttpClient> httpClient => new Lazy<HttpClient>(() => new HttpClient());

        public static async Task<bool> IsHealthyAsync(this InstagramItem item)
        {
            try
            {
                var r = await httpClient.Value.GetAsync(item.ImageUrls[0]);
                r.EnsureSuccessStatusCode();

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}