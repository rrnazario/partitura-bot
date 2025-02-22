using Microsoft.Extensions.DependencyInjection;
using TelegramPartHook.Application.Commands.ClearInformation;
using TelegramPartHook.Application.Factories;

namespace TelegramPartHook.DI
{
    public static class FactoriesDI
    {
        public static IServiceCollection AddFactories(this IServiceCollection services)
        {
            services.AddScoped<IRequestFactory, RequestFactory>();
            services.AddScoped<ISearchFactory, SearchFactory>();
            services.AddScoped<ISearchAccessor, SearchAccessor>();
            services.AddScoped<IUserFactory, UserFactory>();
            services.AddSingleton<IClearInformationFactory, ClearInformationFactory>();

            return services;
        }
    }
}
