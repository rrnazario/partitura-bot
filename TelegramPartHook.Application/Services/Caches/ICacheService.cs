namespace TelegramPartHook.Domain.Services.Caches
{
    public interface ICacheService
    {
        Task HandleAsync(CancellationToken token);
    }
}