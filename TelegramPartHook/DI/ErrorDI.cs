using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using TelegramPartHook.Application.Middlewares;
using TelegramPartHook.Application.Services;

namespace TelegramPartHook.DI
{
    public static class ErrorDI
    {
        public static IServiceCollection AddErrorHandlers(this IServiceCollection services)
        {
            services.AddScoped<IExceptionHandler, ExceptionHandler>();
            services.AddScoped<ErrorCatchingMiddleware>();

            return services;
        }

        public static IApplicationBuilder UseErrorHandlers(this IApplicationBuilder app)
        {
            app.UseMiddleware<ErrorCatchingMiddleware>();

            return app;
        }
    }
}
