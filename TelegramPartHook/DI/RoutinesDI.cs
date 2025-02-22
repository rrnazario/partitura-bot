using Microsoft.Extensions.DependencyInjection;
using TelegramPartHook.Application.Routines;
using TelegramPartHook.Application.Services.Caches;

namespace TelegramPartHook.DI
{
    public static class RoutinesDI
    {
        public static IServiceCollection AddRoutines(this IServiceCollection services)
        {
            services.AddScoped<IInstaUpdateCacheService, InstaUpdateCacheService>();
            services.AddScoped<ISearchCacheCleanerService, SearchCacheCleanerService>();

            services.AddHostedService<AvailabilityMonitorJob>();
            services.AddHostedService<CacheHandlerJob>();

            return services;
        }
    }
}
