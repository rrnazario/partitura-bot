using TelegramPartHook.Domain.SeedWork;

namespace TelegramPartHook.Application.Interfaces.Searches;

public interface ISearchService
{
    Task<IEnumerable<SheetSearchResult>> SearchAsync(string term, CancellationToken cancellationToken);
}