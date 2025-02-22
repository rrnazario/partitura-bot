using Light.GuardClauses;
using Microsoft.EntityFrameworkCore;
using TelegramPartHook.Application.Helpers;
using TelegramPartHook.Domain.Aggregations.ConfigAggregation;
using TelegramPartHook.Domain.Aggregations.InstagramCacheAggregation;
using TelegramPartHook.Domain.Constants;
using TelegramPartHook.Domain.Services.Caches;
using TelegramPartHook.Infrastructure.Persistence;

namespace TelegramPartHook.Application.Services.Caches;

public interface IInstaUpdateCacheService : ICacheService
{
}

public class InstaUpdateCacheService : CacheBaseService, IInstaUpdateCacheService
{
    private readonly HttpClient _httpClient;
    private readonly IGlobalState _state;
    private readonly IAdminConfiguration _adminConfiguration;
    private readonly ILogHelper _log;

    public InstaUpdateCacheService(BotContext context,
        HttpClient httpClient,
        IGlobalState state,
        IAdminConfiguration adminConfiguration,
        ILogHelper log)
        : base(context, log)
    {
        _httpClient = httpClient;
        _state = state.MustNotBeNull();
        _adminConfiguration = adminConfiguration.MustNotBeNull();
        _log = log.MustNotBeNull();
    }

    public override async Task DefineNextTimeToRunAsync(CancellationToken token)
    {
        var nextDate = DateTime.Now.AddDays(7);

        var config = _context.Set<Config>()
            .First(c => c.Name == ConfigDateTimeName.NextDateSearchOnInstagram.ToString());

        config.SetDateTimeValue(nextDate);

        await _context.SaveChangesAsync(token).ConfigureAwait(false);
    }

    public override async Task RunAsync(CancellationToken token)
    {
        var initialExtractor = new InstaInitialExtractor(_httpClient, _adminConfiguration);

        if (initialExtractor.IsValid)
        {
            var username = string.IsNullOrEmpty(initialExtractor.ExtractUserName())
                ? "linhas.espacos"
                : initialExtractor.ExtractUserName();
            var page = _context.Set<InstagramCache>().AsNoTracking().FirstOrDefault(p => p.PageName == username);

            if (page is not null)
            {
                page.UpdateFromExtractor(initialExtractor);

                _context.Attach(page);
                _context.Update(page);
            }
            else
            {
                page = new InstagramCache(initialExtractor);

                await _context.Set<InstagramCache>().AddAsync(page, token);
            }

            await _context.SaveChangesAsync(token);
        }

        _log.Info("It was not possible retrieve Instagram content", token, true);
    }

    public override bool IsTimeToRun()
    {
        var nextDateSearchOnInstagram = _context.Set<Config>()
            .AsNoTracking()
            .First(c => c.Name == ConfigDateTimeName.NextDateSearchOnInstagram.ToString()).GetDateTimeValue();

        return nextDateSearchOnInstagram <= DateTime.UtcNow && !_state.IsDebug;
    }
}