using iText.IO.Image;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Xobject;
using iText.Layout;
using iText.Layout.Element;
using static TelegramPartHook.Domain.Constants.Enums;
using TelegramPartHook.Infrastructure.Helpers;
using iText.Kernel.Utils;
using TelegramPartHook.Domain.SeedWork;
using Light.GuardClauses;
using TelegramPartHook.Application.Helpers;
using TelegramPartHook.Application.Interfaces.Searches;

namespace TelegramPartHook.Application.Services
{
    public interface IPdfService
    {
        Task<string> GenerateAsync(SheetSearchResult[] sheets, string title);
    }

    public class PdfService
        : IPdfService
    {
        private readonly IDropboxService _dropboxService;
        private readonly ISystemHelper _systemHelper;
        private readonly ILogHelper _log;

        public PdfService(IDropboxService dropboxService, ISystemHelper systemHelper, ILogHelper log)
        {
            _dropboxService = dropboxService.MustNotBeNull();
            _systemHelper = systemHelper.MustNotBeNull();
            _log = log.MustNotBeNull();
        }

        public async Task<string> GenerateAsync(SheetSearchResult[] sheets, string title)
        {
            var finalResult = Path.Combine(Path.GetTempPath(), $"{title.RemoveInvalidFileNameChars()}.pdf");

            var result = Path.Combine(Path.GetTempPath(), $"{Path.GetTempFileName()}.pdf");
            if (File.Exists(result))
                File.Delete(result);

            var imageSheets = sheets.Where(_ =>
                !_.Address.EndsWith("pdf") && !_.ServerPath.EndsWith("pdf") &&
                _.Source != FileSource.CrawlerDownloadLink).ToArray();
            if (imageSheets.Any())
            {
                await using var writer = new PdfWriter(result);
                using var pdf = new PdfDocument(writer);
                using var document = new Document(pdf, iText.Kernel.Geom.PageSize.A4);
                foreach (var item in imageSheets)
                {
                    try
                    {
                        switch (item.Source)
                        {
                            case FileSource.Dropbox:
                                await DownloadDropboxFileAsync(item);
                                break;
                            default:
                            {
                                if (!File.Exists(item.LocalPath) && Uri.TryCreate(item.Address, new(), out _))
                                {
                                    item.SetLocalPath(await _systemHelper.DownloadFileAsync(item.Address));
                                }

                                break;
                            }
                        }

                        var imgData = ImageDataFactory.Create(filename: item.LocalPath);
                        var imgObject = new PdfImageXObject(imgData);
                        var img = new Image(imgObject);

                        document.Add(img);
                    }
                    catch (Exception e)
                    {
                        _log.Info($"{item}\n\n{e.Message}\n\n{e.StackTrace}", CancellationToken.None,
                            sendToAdmin: true);
                    }
                }

                document.Close();
            }

            var pdfSheets = sheets.Where(sheet =>
                    sheet.Address.EndsWith("pdf") || sheet.ServerPath.EndsWith("pdf") ||
                    sheet.Source == FileSource.CrawlerDownloadLink)
                .ToList();
            if (pdfSheets.Any())
            {
                result = await MergePdfsAsync(pdfSheets, result);
            }

            File.Move(result, finalResult, overwrite: true);

            return finalResult;
        }

        private async Task<string> MergePdfsAsync(List<SheetSearchResult> pdfSheets, string result)
        {
            if (File.Exists(result))
                pdfSheets.Add(new SheetSearchResult(result, FileSource.Generated));

            result = Path.Combine(Path.GetTempPath(), $"{Path.GetTempFileName()}.pdf");

            using var writerMemoryStream = new MemoryStream();
            await using var pdfWriter = new PdfWriter(result);
            using var mergedDocument = new PdfDocument(pdfWriter);

            var merger = new PdfMerger(mergedDocument);

            foreach (var sheet in pdfSheets)
            {
                if (!sheet.Exists)
                {
                    switch (sheet.Source)
                    {
                        case FileSource.Dropbox:
                            await DownloadDropboxFileAsync(sheet);
                            break;
                        case FileSource.CrawlerDownloadLink:
                            await _systemHelper.DownloadFileIfNeededAsync(sheet, "pdf");
                            break;
                        default:
                            sheet.SetLocalPath(await _systemHelper.DownloadFileAsync(sheet.Address, "pdf"));
                            break;
                    }
                }

                using var copyFromMemoryStream = new MemoryStream(await File.ReadAllBytesAsync(sheet.LocalWhereExists));
                using var reader = new PdfReader(copyFromMemoryStream);
                using var copyFromDocument = new PdfDocument(reader);

                merger.Merge(copyFromDocument, 1, copyFromDocument.GetNumberOfPages());
            }

            merger.Close();

            return result;
        }

        private async Task DownloadDropboxFileAsync(SheetSearchResult file)
        {
            if (!file.Exists)
            {
                var content = await _dropboxService.DownloadFileAsync(file.ServerPath);

                HandleName(file);

                await File.WriteAllBytesAsync(file.LocalPath, content);
            }
        }

        private static void HandleName(SheetSearchResult file)
        {
            if (string.IsNullOrEmpty(file.LocalPath))
                file.SetLocalPath(Path.Combine(Path.GetTempPath(), file.Address));
            else
            {
                var fileInfo = new FileInfo(file.LocalPath);
                var newName = Path.Combine(fileInfo.DirectoryName, fileInfo.Name.RemoveInvalidFileNameChars());
                file.SetLocalPath(newName.Length > 250
                    ? Path.Combine(fileInfo.DirectoryName, $"{Path.GetTempFileName()}{fileInfo.Extension}")
                    : newName);
            }
        }
    }
}