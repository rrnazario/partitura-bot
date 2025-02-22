using System.Net.Http;

namespace TelegramPartHook.DI;

public static class DIExtensions
{
    public static void ConfigureHeaders(this HttpClient config)
    {
        config.DefaultRequestHeaders.Add("Accept",
            "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
        config.DefaultRequestHeaders.Add("User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML,  like Gecko) Chrome/89.0.4389.82 Safari/537.36");
    }
}