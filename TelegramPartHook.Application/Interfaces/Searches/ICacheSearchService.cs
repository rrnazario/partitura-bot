using TelegramPartHook.Domain.SeedWork;

namespace TelegramPartHook.Application.Interfaces.Searches;

public interface ICacheSearchService : ISearchService
{
    Task<bool> SaveResultsAsync(string term, SheetSearchResult[] results, CancellationToken cancellationToken);
}