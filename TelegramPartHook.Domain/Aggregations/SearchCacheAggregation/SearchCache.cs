using TelegramPartHook.Domain.SeedWork;

namespace TelegramPartHook.Domain.Aggregations.SearchCacheAggregation;

public class SearchCache
    : Entity
{
    public string Term { get; private set; }
    public SheetSearchResult[] Results { get; private set; }
    
    public string AddedDate { get; set; }
    
    private SearchCache() { }

    public SearchCache(string term, SheetSearchResult[] results)
    {
        Term = term;

        foreach (var result in results)
        {
            result.FillId();
        }

        Results = results;
    }
}