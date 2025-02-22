using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TelegramPartHook.Application.Interfaces;
using TelegramPartHook.Domain.Aggregations.ReportImageAggregation;
using TelegramPartHook.Domain.SeedWork;
using TelegramPartHook.Infrastructure.Persistence;

namespace TelegramPartHook.Application.Services;

public class SanitizeService(IServiceScopeFactory serviceScopeFactory)
    : ISanitizeService
{
    public async Task<List<SheetSearchResult>> TrySanitizeResultsAsync(List<SheetSearchResult> results)
    {
        results = results
            .Where(w => !w.Address.Contains("capa", StringComparison.InvariantCultureIgnoreCase))
            .Distinct()
            .ToList();

        var urls = results.Select(s => s.Address).ToArray();

        var reports = await GetReportImagesAsync(urls);

        if (reports.Length != 0)
        {
            results = results.Where(r => reports.All(report => report.Url != r.Address)).ToList();
        }

        return results;
    }

    private async Task<ReportImage[]> GetReportImagesAsync(string[] urls)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BotContext>();

        return await context
            .Set<ReportImage>()
            .AsNoTracking()
            .Where(r => urls.Contains(r.Url) && r.IsActive)
            .ToArrayAsync();
    }
}