using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using TelegramPartHook.Application.Services;

namespace TelegramPartHook.Application.Middlewares
{
    public class ErrorCatchingMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                var exceptionHandler = context.RequestServices.GetRequiredService<IExceptionHandler>();

                await exceptionHandler.HandleAsync(ex);
            }
        }
    }
}
