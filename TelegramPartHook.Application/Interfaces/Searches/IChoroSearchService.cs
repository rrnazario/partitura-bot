using TelegramPartHook.Domain.SeedWork;

namespace TelegramPartHook.Application.Interfaces.Searches
{
    public interface IChoroSearchService : ISearchService
    {
        Task<IEnumerable<SheetSearchResult>> SearchChorosOnCasaDoChoroPdfAsync(string term);
        Task<IEnumerable<SheetSearchResult>> SearchChorosOnCantoriumPdfAsync(string term);
    }
}
