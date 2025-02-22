using System.Threading;
using TelegramPartHook.Domain.SeedWork;

namespace TelegramPartHook.Domain.Aggregations.SearchCacheAggregation;

public interface ISearchCacheRepository : IRepository<SearchCache>
{
    Task<SearchCache> GetByTermAsync(string term, CancellationToken cancellationToken = default);
}