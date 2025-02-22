using System.Globalization;
using Microsoft.EntityFrameworkCore;
using TelegramPartHook.Application.Helpers;
using TelegramPartHook.Domain.Aggregations.ConfigAggregation;
using TelegramPartHook.Domain.Aggregations.SearchCacheAggregation;
using TelegramPartHook.Domain.Constants;
using TelegramPartHook.Domain.Services.Caches;
using TelegramPartHook.Infrastructure.Persistence;

namespace TelegramPartHook.Application.Services.Caches;

public interface ISearchCacheCleanerService : ICacheService
{
};

public class SearchCacheCleanerService : CacheBaseService, ISearchCacheCleanerService
{
    public SearchCacheCleanerService(BotContext context, ILogHelper log)
        : base(context, log)
    {
    }

    public override async Task DefineNextTimeToRunAsync(CancellationToken token)
    {
        var nextDate = DateTime.UtcNow.AddDays(1);

        var config = _context.Set<Config>()
            .First(c => c.Name == ConfigDateTimeName.NextDateToCacheClear.ToString());

        config.SetDateTimeValue(nextDate);

        await _context.SaveChangesAsync(token);
    }

    public override async Task RunAsync(CancellationToken token)
    {
        var caches = _context.Set<SearchCache>()
            .ToArray();

        var cachesToRemove = caches.Where(c =>
                DateTime.ParseExact(c.AddedDate, DateConstants.DatabaseFormat, new CultureInfo("pt-BR"))
                    .AddDays(14) <
                DateTime.UtcNow)
            .ToArray();

        await _logHelper.SendMessageToAdminAsync($"Removing {cachesToRemove.Length} cache keys...", token);

        _context.RemoveRange(cachesToRemove);
        await _context.SaveChangesAsync(token);
    }

    public override bool IsTimeToRun()
    {
        var nextDateToCacheClear = _context.Set<Config>().AsNoTracking()
            .First(c => c.Name == ConfigDateTimeName.NextDateToCacheClear.ToString()).GetDateTimeValue();
        return nextDateToCacheClear <= DateTime.UtcNow;
    }
}