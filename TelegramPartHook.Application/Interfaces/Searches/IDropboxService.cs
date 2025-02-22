using TelegramPartHook.Domain.SeedWork;

namespace TelegramPartHook.Application.Interfaces.Searches
{
    public interface IDropboxService : ISearchService
    {
        Task<IEnumerable<SheetSearchResult>> GetAllAsync(CancellationToken cancellationToken);
        
        Task<bool> UploadFileAsync(string content, string name, CancellationToken cancellationToken);

        Task UploadFilesAsync(string[] filePaths, string prefix);

        Task<Dictionary<string, bool>> FilesExistAsync(string[] fileNames);
        
        Task<bool> DeleteFileAsync(string file);
        
        Task<byte[]> DownloadFileAsync(string name);
    }
}
