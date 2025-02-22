using MediatR;
using TelegramPartHook.Application.Interfaces.Searches;
using TelegramPartHook.Domain.Aggregations.UserAggregation.DomainEvents;
using TelegramPartHook.Infrastructure.Helpers;
using static TelegramPartHook.Domain.Constants.Enums;

namespace TelegramPartHook.Application.Notifications;

public class InstagramDownloadNotificationHandler
    : INotificationHandler<BackupFilesDomainEvent>
{
    private readonly IDropboxService _dropboxService;
    private readonly ISystemHelper _systemHelper;

    public InstagramDownloadNotificationHandler(
        IDropboxService dropboxService,
        ISystemHelper systemHelper)
    {
        _dropboxService = dropboxService;
        _systemHelper = systemHelper;
    }

    public async Task Handle(BackupFilesDomainEvent notification, CancellationToken cancellationToken)
    {
        var instaFiles = notification.Sheets.Where(p => p.Source == FileSource.Instagram).ToArray();

        if (!instaFiles.Any()) return;

        var groupedItems = notification.Sheets.GroupBy(result => result.AdditionalInfo).ToArray();

        var exists = await _dropboxService.FilesExistAsync(groupedItems.Select(grouping => grouping.Key).ToArray());

        foreach (var item in groupedItems.Where(i => !exists[i.Key]).ToArray())
        {
            var downloadedFiles = new List<string>();

            downloadedFiles.AddRange(
                item.Select(result =>
                _systemHelper.DownloadFileAsync(result.Address).GetAwaiter().GetResult()
            ));

            await _dropboxService.UploadFilesAsync(downloadedFiles.ToArray(), item.Key);
        }
    }
}