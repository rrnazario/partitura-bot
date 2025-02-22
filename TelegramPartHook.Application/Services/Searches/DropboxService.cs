using Dropbox.Api;
using Dropbox.Api.Files;
using Microsoft.Extensions.Caching.Memory;
using TelegramPartHook.Application.Helpers;
using TelegramPartHook.Application.Interfaces.Searches;
using TelegramPartHook.Domain.Constants;
using TelegramPartHook.Domain.SeedWork;
using TelegramPartHook.Infrastructure.Helpers;
using static TelegramPartHook.Domain.Constants.Enums;

namespace TelegramPartHook.Application.Services.Searches;

public class DropboxService : IDropboxService
{
    private readonly DropboxClientConfig _dropboxClientConfig;
    private readonly IMemoryCache _cache;
    private readonly IAdminConfiguration _adminConfiguration;
    private const string GetAllCacheKey = $"{nameof(DropboxService)}.{nameof(GetAllAsync)}";

    public const string PartituraDropboxPath = "/Partituras";

    public DropboxService(
        IHttpClientFactory httpClientFactory,
        IMemoryCache cache,
        IAdminConfiguration adminConfiguration)
    {
        _dropboxClientConfig = new DropboxClientConfig
        {
            HttpClient = httpClientFactory.CreateClient(),
            MaxRetriesOnError = 3
        };

        _cache = cache;
        _adminConfiguration = adminConfiguration;
    }

    public async Task<IEnumerable<SheetSearchResult>> GetAllAsync(CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue<IEnumerable<SheetSearchResult>>(GetAllCacheKey, out var result))
            return result;

        using var client = CreateClient();

        var list = await client.Files.ListFolderAsync(PartituraDropboxPath);

        var search = list.Entries.Where(wh => wh.IsFile).ToArray();

        var metadatas = new List<Metadata>();
        var pathImages = new List<SheetSearchResult>();

        if (search.Any() || list.HasMore)
        {
            metadatas.AddRange(search);

            while (list.HasMore)
            {
                list = await client.Files.ListFolderContinueAsync(list.Cursor);

                metadatas.AddRange(list.Entries.Where(wh => wh.IsFile));
            }
        }

        metadatas.ForEach(item => pathImages.Add(new(item.AsFile.Name, FileSource.Dropbox, item.AsFile.PathLower)));

        pathImages = pathImages.OrderBy(o => o.Address).ToList();
        _cache.Set<IEnumerable<SheetSearchResult>>(GetAllCacheKey, pathImages);

        return pathImages;
    }

    public async Task<bool> UploadFileAsync(string content, string name, CancellationToken cancellationToken)
    {
        using var streamContent = ImageParser.FromBase64ToStream(content);

        using var client = CreateClient();
        await UploadFileAsync(client, name, streamContent);

        return true;
    }

    public async Task UploadFilesAsync(string[] filePaths, string prefix)
    {
        if (filePaths.Length == 0) return;

        using var client = CreateClient();

        var count = 1;
        foreach (var filePath in filePaths)
        {
            if (!File.Exists(filePath))
            {
                continue;
            }

            await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            var fileInfo = new FileInfo(filePath);
            var serverName = $"{prefix} {count}{fileInfo.Extension}";

            await UploadFileAsync(client, serverName, stream);

            count++;
        }
    }

    public async Task<IEnumerable<SheetSearchResult>> SearchAsync(string term, CancellationToken cancellationToken)
    {
        using var client = CreateClient();
        var search = await client.Files.SearchV2Async(term, new(PartituraDropboxPath));

        if (!search.Matches.Any()) return [];

        var metadatas = new List<MetadataV2.Metadata>();
        var pathImages = new List<SheetSearchResult>();

        metadatas.AddRange(search.Matches.Select(m => (MetadataV2.Metadata)m.Metadata));

        if (search.HasMore)
        {
            while (search.HasMore)
            {
                search = await client.Files.SearchContinueV2Async(search.Cursor);
                metadatas.AddRange(search.Matches.Select(_ => (MetadataV2.Metadata)_.Metadata));
            }
        }

        metadatas.ForEach(item =>
            pathImages.Add(new SheetSearchResult(item.Value.Name, FileSource.Dropbox, item.Value.PathLower)));

        return pathImages;
    }
    
    public async Task<Dictionary<string, bool>> FilesExistAsync(string[] fileNames)
    {
        using var client = CreateClient();

        var list = await client.Files.ListFolderAsync(PartituraDropboxPath);

        var search = list.Entries.Where(wh => wh.IsFile).ToArray();

        var result = fileNames.ToDictionary(k => k,
            v => search.Any(s =>
                s.Name.ReplaceDiacritics()
                    .StartsWith(v.ReplaceDiacritics(), StringComparison.InvariantCultureIgnoreCase)));

        while (list.HasMore && result.Any(v => !v.Value))
        {
            list = await client.Files.ListFolderContinueAsync(list.Cursor);
            search = list.Entries.Where(wh => wh.IsFile).ToArray();

            var inexistents = result.Where(w => !w.Value).ToList();
            inexistents.ForEach(f =>
                result[f.Key] = search.Any(s =>
                    s.Name.ReplaceDiacritics().StartsWith(f.Key.ReplaceDiacritics(),
                        StringComparison.InvariantCultureIgnoreCase))
            );
        }

        return result;
    }
    
    public async Task<bool> DeleteFileAsync(string file)
    {
        if (string.IsNullOrEmpty(file)) return false;

        using var client = CreateClient();

        var result = await client.Files.DeleteV2Async(file);

        return result.Metadata.IsDeleted;
    }

    /// <summary>
    /// File will be created at file.LocalPath
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public async Task<byte[]> DownloadFileAsync(string name)
    {
        using var client = CreateClient();

        using var response = await client.Files.DownloadAsync(name);

        return await response.GetContentAsByteArrayAsync();
    }    

    private DropboxClient CreateClient() => new(_adminConfiguration.DropboxToken, _dropboxClientConfig);

    private static async Task UploadFileAsync(DropboxClient client, string serverName, Stream stream)
    {
        await client.Files.UploadAsync(Path.Combine(PartituraDropboxPath, serverName).Replace("\\", "/"),
            body: stream,
            mode: WriteMode.Overwrite.Instance);
    }
}