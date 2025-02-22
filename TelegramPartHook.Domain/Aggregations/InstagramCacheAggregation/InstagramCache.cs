using System.Text.Json.Serialization;
using Light.GuardClauses;
using TelegramPartHook.Domain.SeedWork;

namespace TelegramPartHook.Domain.Aggregations.InstagramCacheAggregation
{
    public class InstagramCache
        : Entity
    {
        public string PageName { get; private set; }

        public InstagramItem[] Items { get; private set; }

        public long GraphqlId { get; private set; }

        public string EndCursor { get; private set; }

        [JsonConstructor]
        private InstagramCache() { }

        public InstagramCache(InstaInitialExtractor extractor)
            => UpdateFromExtractor(extractor);

        public InstagramCache(InstaInitialExtractor extractor, InstaPaginatedExtractor paginatedExtractor)
        : this(extractor)
        => UpdateFromExtractor(paginatedExtractor);

        public void UpdateFromExtractor(InstaInitialExtractor extractor)
        {
            extractor.MustNotBeNull();

            UpdateItems(extractor.ExtractInstagramItems().ToArray());
            UpdateGraphqlId(extractor.ExtractGraphqlId());
            UpdateEndCursor(extractor.ExtractEndCursor());
            UpdatePageName(extractor.ExtractUserName());
        }

        public void UpdateFromExtractor(InstaPaginatedExtractor extractor)
        {
            extractor.MustNotBeNull();

            UpdateItems(extractor);
            //UpdateEndCursor(extractor.ExtractEndCursor());
        }

        public void UpdateEndCursor(string endCursor) => EndCursor = endCursor;

        public void UpdateItems(InstagramItem[] items)
        {
            Items = items.MustNotBeNullOrEmpty();
        }

        public void UpdateItems(InstaPaginatedExtractor extractor)
        {
            var newItems = Items.ToList();
            newItems.AddRange(extractor.ExtractInstagramItems());
            Items = newItems.ToArray();
        }

        public void UpdateGraphqlId(long id) => GraphqlId = id;
        public void UpdatePageName(string name) => PageName = name;
    }
}
