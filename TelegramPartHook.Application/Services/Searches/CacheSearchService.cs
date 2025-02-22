using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using TelegramPartHook.Application.Helpers;
using TelegramPartHook.Application.Interfaces.Searches;
using TelegramPartHook.Domain.Aggregations.SearchCacheAggregation;
using TelegramPartHook.Domain.SeedWork;
using TelegramPartHook.Infrastructure.Persistence;

namespace TelegramPartHook.Application.Services.Searches;

public class CacheSearchService : ICacheSearchService
{
    private readonly IServiceScopeFactory _factory;
    private readonly IMemoryCache _memoryCache;

    public CacheSearchService(IServiceScopeFactory factory, IMemoryCache memoryCache)
    {
        _factory = factory;
        _memoryCache = memoryCache;
    }

    public async Task<IEnumerable<SheetSearchResult>> SearchAsync(string term, CancellationToken cancellationToken)
    {
        if (_memoryCache.TryGetValue<List<SheetSearchResult>>(term, out var sheetSearchResults))
            return sheetSearchResults!;

        SearchCache? search = null;
        try
        {
            using var scope = _factory.CreateScope();
            var searchCacheRepository = scope.ServiceProvider.GetRequiredService<ISearchCacheRepository>();

            search = await searchCacheRepository.GetByTermAsync(term, cancellationToken);
        }
        catch
        {
            Log.Error("It was not possible to get from cache");
        }

        return search?.Results ?? [];
    }

    public async Task<bool> SaveResultsAsync(string term, SheetSearchResult[] results, CancellationToken cancellationToken)
    {
        await using var scope = _factory.CreateAsyncScope();
        var log = scope.ServiceProvider.GetRequiredService<ILogHelper>();

        try
        {
            var searchCacheRepository = scope.ServiceProvider.GetRequiredService<ISearchCacheRepository>();

            var search = await searchCacheRepository.GetByTermAsync(term, cancellationToken);

            if (search is null)
            {
                search = new SearchCache(term, results);
                searchCacheRepository.Add(search);
                await searchCacheRepository.SaveChangesAsync(cancellationToken);
                
                return true;
            }

            AddOnMemoryCache(term, results);
        }
        catch (Exception e)
        {
            await log.ErrorAsync(e, cancellationToken);
        }

        return false;
    }

    private void AddOnMemoryCache(string term, SheetSearchResult[] results)
    {
        if (results.Length <= 5)
            _memoryCache.Set(term, results);
    }
}