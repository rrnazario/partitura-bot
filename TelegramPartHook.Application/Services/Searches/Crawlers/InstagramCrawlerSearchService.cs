using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TelegramPartHook.Application.Interfaces.Searches;
using TelegramPartHook.Domain.Aggregations.ConfigAggregation;
using TelegramPartHook.Domain.Aggregations.InstagramCacheAggregation;
using TelegramPartHook.Domain.Constants;
using TelegramPartHook.Domain.SeedWork;
using TelegramPartHook.Infrastructure.Helpers;
using TelegramPartHook.Infrastructure.Helpers.Instagram;
using TelegramPartHook.Infrastructure.Persistence;

namespace TelegramPartHook.Application.Services.Searches.Crawlers
{
    public class InstagramCrawlerSearchService
       : IInstagramCrawlerSearchService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public InstagramCrawlerSearchService(IServiceScopeFactory scopeFactory)
            => _scopeFactory = scopeFactory;

        public async Task<IEnumerable<SheetSearchResult>> SearchAsync(string term, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(term))
                throw new ArgumentException("termo");

            var returnImages = new List<SheetSearchResult>();

            var foundItems = GetInstagramItems()
                        .Where(w => w.Text.Contains(term, StringComparison.InvariantCultureIgnoreCase))
                        .ToArray();

            if (foundItems.Any() && !(await foundItems[0].IsHealthyAsync()))
            {
                await MarkCacheAsOutdatedAsync();
                return Array.Empty<SheetSearchResult>();
            }

            foreach (var item in foundItems)
            {
                var aditionalInfo = item.Text.RemoveInvalidFileNameChars();

                aditionalInfo = aditionalInfo.Substring(0, Math.Min(230, aditionalInfo.Length));

                returnImages.AddRange(item.ImageUrls
                    .Select(url => new SheetSearchResult(url,
                    Enums.FileSource.Instagram,
                    additionalInfo: aditionalInfo)));
            }

            return returnImages.Distinct();
        }

        private async Task MarkCacheAsOutdatedAsync()
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<BotContext>();

            var config = context.Set<Config>().First(c => c.Name == ConfigDateTimeName.NextDateSearchOnInstagram.ToString());
            config.SetDateTimeValue(DateTime.UtcNow.AddDays(-1));

            await context.SaveChangesAsync();
        }

        private InstagramItem[] GetInstagramItems()
        {
            using var scope = _scopeFactory.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<BotContext>();

            var cache = context.Set<InstagramCache>().AsNoTracking().FirstOrDefault();

            return cache is null
            ? Array.Empty<InstagramItem>()
            : cache.Items.ToArray();
        }

    }
}