using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TelegramPartHook.Application.Helpers;
using TelegramPartHook.Application.Notifications;
using TelegramPartHook.Application.Services.Caches;
using TelegramPartHook.Domain.Services.Caches;

namespace TelegramPartHook.Application.Routines
{
    public class CacheHandlerJob(ILogHelper log, IServiceScopeFactory scopeFactory, IMediator mediator)
        : IHostedService, IDisposable
    {
        private readonly CancellationTokenSource _cts = new();
        private Task? _executingTask;

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
                    foreach (var routine in GetRoutines())
                    {
                        log.Info($"Running {routine.GetType().FullName}", token);

                        await routine.HandleAsync(token);
                    }
                }
                catch (Exception e)
                {
                    await log.ErrorAsync(e, token);
                    await mediator.Publish(new SaveErrorEvent(e), token);
                }

                await Task.Delay(TimeSpan.FromDays(1), token);
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

        private IEnumerable<ICacheService> GetRoutines()
        {
            using var scope = scopeFactory.CreateScope();
            yield return scope.ServiceProvider.GetRequiredService<ISearchCacheCleanerService>();

            //yield return scope.ServiceProvider.GetRequiredService<IInstaUpdateCacheService>();
        }
    }
}