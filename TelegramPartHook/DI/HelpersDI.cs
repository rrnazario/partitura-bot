using Microsoft.Extensions.DependencyInjection;
using TelegramPartHook.Application.Helpers;
using TelegramPartHook.Application.Interfaces;
using TelegramPartHook.Application.Services;
using TelegramPartHook.Infrastructure.Helpers;

namespace TelegramPartHook.DI
{
    public static class HelpersDI
    {
        public static IServiceCollection AddHelpers(this IServiceCollection services)
        {
            services.AddSingleton<ITelegramSender, TelegramSender>();
            services.AddSingleton<ILogHelper, LogHelper>();
            services.AddSingleton<IGlobalState, GlobalState>();
            services.AddSingleton<IPdfService, PdfService>();
            services.AddHttpClient<ISystemHelper, SystemHelper>(config => config.ConfigureHeaders());

            services.AddSingleton<ISanitizeService, SanitizeService>();

            services.AddScoped<ILoginService, LoginService>();

            return services;
        }
    }
}