using TelegramPartHook.Application.DTO;

namespace TelegramPartHook.Application.Interfaces
{
    public interface IUtilitaryService
    {
        Task DownloadYoutubeMp3Async(Search pesquisa);
    }
}
