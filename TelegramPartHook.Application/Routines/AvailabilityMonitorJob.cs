using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TelegramPartHook.Application.Helpers;
using TelegramPartHook.Application.Notifications;
using TelegramPartHook.Application.Queries;
using TelegramPartHook.Application.Services;
using TelegramPartHook.Domain.Aggregations.ConfigAggregation;
using TelegramPartHook.Domain.Aggregations.UserAggregation;
using TelegramPartHook.Domain.SeedWork;
using TelegramPartHook.Infrastructure.Persistence;
using static TelegramPartHook.Domain.Constants.Enums;

namespace TelegramPartHook.Application.Routines;

public class AvailabilityMonitorJob : IHostedService, IDisposable
{
    private readonly CancellationTokenSource _cts = new();
    private readonly IMediator _mediator;
    private readonly ITelegramSender _sender;
    private readonly ILogHelper _log;
    private BotContext? _context;
    private Task? _executingTask;

    private readonly Lock _locker = new();

    private readonly IServiceScopeFactory _scopeFactory;

    public AvailabilityMonitorJob(ITelegramSender sender,
        ILogHelper log,
        IMediator mediator,
        IServiceScopeFactory scopeFactory)
    {
        _sender = sender;
        _log = log;
        _mediator = mediator;
        _scopeFactory = scopeFactory;
    }

    public void Dispose()
    {
        _cts.Cancel();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _executingTask = MonitorAsync(_cts.Token);

        return _executingTask.IsCompleted
            ? _executingTask
            : Task.CompletedTask;
    }

    private async Task MonitorAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                _context = scope.ServiceProvider.GetRequiredService<BotContext>();

                if (!IsTimeToRun()) return;

                var configNews = _context.Set<Config>().AsNoTracking();

                var nextDateToMonitorRun =
                    configNews.First(c => c.Name == ConfigDateTimeName.NextDateToMonitorRun.ToString())
                        .GetDateTimeValue();

                _log.Info($"Running based on config date: '{nextDateToMonitorRun:dd-MM-yyyy HH:mm:ss}'",
                    token);

                var scheduledUsers = GetScheduledUsers();

                foreach (var user in scheduledUsers)
                    await PerformSearchForUserAsync(user, token);

                DefineNextRunDate(token);
            }
            catch (DbUpdateConcurrencyException e)
            {
                await _mediator.Publish(new SaveErrorEvent(e), token);
            }
            catch (Exception e)
            {
                await _log.ErrorAsync(e, token);
            }

            await Task.Delay(TimeSpan.FromHours(1), token);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        // Stop called without start
        if (_executingTask is null)
            return;

        try
        {
            await _cts.CancelAsync();
        }
        finally
        {
            await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
        }
    }

    private bool IsTimeToRun()
    {
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development") return false;

        var config = _context!.Set<Config>().AsNoTracking();
        var nextDateToMonitorRun =
            config.First(c => c.Name == ConfigDateTimeName.NextDateToMonitorRun.ToString()).GetDateTimeValue();
        return nextDateToMonitorRun <= DateTime.UtcNow;
    }

    private User[] GetScheduledUsers()
    {
        var allScheduledUsers = _context!.Set<User>().AsNoTracking()
            .TagWith("Getting scheduled users")
            .Where(u => !string.IsNullOrEmpty(u.searchesscheduled));

        return allScheduledUsers.ToArray();
    }

    private void DefineNextRunDate(CancellationToken token)
    {
        lock (_locker)
        {
            var nextDateToMonitorRun =
                _context!.Set<Config>()
                    .First(c => c.Name == ConfigDateTimeName.NextDateToMonitorRun.ToString());

            nextDateToMonitorRun.SetDateTimeValue(DateTime.UtcNow.AddHours(12));

            _context.SaveChangesAsync(token).GetAwaiter().GetResult();
        }
    }

    private async Task PerformSearchForUserAsync(User user, CancellationToken token)
    {
        _log.Info($"[START] Monitoring user {user.fullname}...", token);

        var monitoredItems = user.GetMonitoredItems();

        var keptItems = new List<string>();

        var toBeMonitored = monitoredItems.Where(w => w.IsValid);
        if (!user.IsVipValid())
            toBeMonitored = toBeMonitored.Where(w => w.SearchedDate.AddYears(1) >= DateTime.UtcNow);

        foreach (var monitoredItem in toBeMonitored)
        {
            _log.Info($"Pesquisando termo '{monitoredItem.Term}'...", token);

            var parts = (await _mediator.Send(new GetSheetLinksQuery(monitoredItem.Term, user.IsVipValid()), token))
                .ToArray();

            if (parts.Length != 0)
                await SendFoundFilesAsync(user, monitoredItem, parts, token);
            else
                keptItems.Add(monitoredItem.ToString());
        }

        await UpdateUserAsync(user, keptItems, monitoredItems, token);

        _log.Info($"[END] Monitoring user {user.fullname}.", token);
    }

    private async Task SendFoundFilesAsync(User user, MonitoredItem monitoredItem, SheetSearchResult[] sheets,
        CancellationToken token)
    {
        await _sender.SendTextMessageAsync(user, MessageName.RememberFound, token,
            placeholders:
            [
                user.fullname, monitoredItem.Term,
                monitoredItem.SearchedDate.ToString("dd/MM/yyyy")
            ]);

        await _sender.SendFilesAsync(sheets, user, false, token);
    }

    private async Task UpdateUserAsync(User user, List<string> keptItems, MonitoredItem[] monitoredItems,
        CancellationToken token)
    {
        if (keptItems.Count != monitoredItems.Length)
        {
            user.UpdateScheduledSearch(keptItems);
            _context!.Update(user);
            await _context.SaveChangesAsync(token);

            await _sender.SendTextMessageAsync(user, MessageName.SeeYou, token);
        }
    }
}