using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Light.GuardClauses;
using TelegramPartHook.Domain.Constants;
using TelegramPartHook.Domain.SeedWork;

namespace TelegramPartHook.Infrastructure.Helpers
{
    public interface ISystemHelper
    {
        bool DeleteFile(params string[] filePathes);

        bool DeleteFolder(string folderPath);

        Task<string> DownloadFileAsync(string url, string extension = "", string fileName = "");

        Task DownloadFileIfNeededAsync(SheetSearchResult result, string extension = "");
    }

    public class SystemHelper
        : ISystemHelper
    {
        private readonly HttpClient _httpClient;

        public SystemHelper(HttpClient httpClientFactory)
        {
            _httpClient = httpClientFactory.MustNotBeNull();
        }

        public bool DeleteFile(params string[] filePathes)
        {
            foreach (var filePath in filePathes)
            {
                try
                {
                    if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                        File.Delete(filePath);
                }
                catch { }
            }

            return true;
        }

        public bool DeleteFolder(string folderPath)
        {
            try
            {
                if (!string.IsNullOrEmpty(folderPath) && Directory.Exists(folderPath))
                    Directory.Delete(folderPath, true);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<string> DownloadFileAsync(string url, string extension = "", string fileName = "")
        {
            var newName = MountNewName(extension, fileName);

            if (File.Exists(newName))
            {
                return newName;
            }

            await using var s = await _httpClient.GetStreamAsync(new Uri(url));
            await using var fs = new FileStream(newName!, FileMode.CreateNew);

            await s.CopyToAsync(fs);

            return newName;
        }

        private static string MountNewName(string extension, string fileName)
        {
            var newName = Path.Combine(Path.GetTempPath(),
                $"{(string.IsNullOrEmpty(fileName) ? Path.GetTempFileName() : fileName.RemoveInvalidFileNameChars())}.{(string.IsNullOrEmpty(extension) ? "jpg" : extension)}");

            return newName;
        }

        public async Task DownloadFileIfNeededAsync(SheetSearchResult result, string extension = "")
        {
            if (result.Source != Enums.FileSource.CrawlerDownloadLink)
                return;

            var newFile = await DownloadFileAsync(result.Address, extension, result.AdditionalInfo);
            result.SetLocalPath(newFile);
        }
    }
}