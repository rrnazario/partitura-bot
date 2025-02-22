using TelegramPartHook.Application.Helpers;
using TelegramPartHook.Infrastructure.Persistence;

namespace TelegramPartHook.Domain.Services.Caches
{
    public abstract class CacheBaseService : ICacheService
    {
        protected readonly BotContext _context;
        protected readonly ILogHelper _logHelper;

        public CacheBaseService(BotContext context, ILogHelper logHelper)
        {
            _context = context;
            _logHelper = logHelper;
        }

        public abstract Task DefineNextTimeToRunAsync(CancellationToken token);

        public async Task HandleAsync(CancellationToken token)
        {
            if (!IsTimeToRun()) return;

            await RunAsync(token);

            await DefineNextTimeToRunAsync(token);
        }

        public abstract bool IsTimeToRun();

        public abstract Task RunAsync(CancellationToken token);
    }
}