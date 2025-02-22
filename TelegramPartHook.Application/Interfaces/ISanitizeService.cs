using TelegramPartHook.Domain.SeedWork;

namespace TelegramPartHook.Application.Interfaces;

public interface ISanitizeService
{
    /// <summary>
    /// Try removing URL with terms that evidence not being a score sheet (e.g. "capa")
    /// </summary>
    /// <param name="results"></param>
    /// <returns></returns>
    Task<List<SheetSearchResult>> TrySanitizeResultsAsync(List<SheetSearchResult> results);
}