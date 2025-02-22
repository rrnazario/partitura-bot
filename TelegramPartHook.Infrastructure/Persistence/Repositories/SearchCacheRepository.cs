using System.Threading;
using System.Threading.Tasks;
using TelegramPartHook.Domain.Aggregations.SearchCacheAggregation;

namespace TelegramPartHook.Infrastructure.Persistence.Repositories;

public class SearchCacheRepository(BotContext context) 
    : RepositoryBase<SearchCache>(context), ISearchCacheRepository
{
    public Task<SearchCache> GetByTermAsync(string term, CancellationToken cancellationToken = default)
        => GetSingleAsync(search => search.Term == term, cancellationToken);
}