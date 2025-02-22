using Microsoft.Extensions.DependencyInjection;
using TelegramPartHook.Application.Interfaces.Searches;
using TelegramPartHook.Application.Services.Searches;
using TelegramPartHook.Application.Services.Searches.Crawlers;

namespace TelegramPartHook.DI
{
    public static class CrawlersDI
    {
        public static IServiceCollection AddSearchers(this IServiceCollection services)
        {
            services.AddSingleton<ICrawlerSearchService, CrawlerSearchService>();
            services
                .AddSingleton<IPagodeAudioPartituraCrawlerSearchService, PagodeAudioPartituraCrawlerSearchService>();

            services.AddSingleton<ISearchService, BlogspotCrawlerSearchService>();
            services.AddSingleton<ISearchService, NandinhoCrawlerSearchService>();
            services.AddHttpClient<IBrasilSonoroCrawlerService, BrasilSonoroCrawlerService>(config =>
                config.ConfigureHeaders());

            services.AddSingleton<ICacheSearchService, CacheSearchService>();
            services.AddSingleton<IDropboxService, DropboxService>();
            services.AddSingleton<IChoroSearchService, ChoroSearchService>();
            services.AddSingleton<IInstagramCrawlerSearchService, InstagramCrawlerSearchService>();

            return services;
        }
    }
}