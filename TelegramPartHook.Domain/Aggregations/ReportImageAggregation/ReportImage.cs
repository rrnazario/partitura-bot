using TelegramPartHook.Domain.Constants;
using TelegramPartHook.Domain.SeedWork;

namespace TelegramPartHook.Domain.Aggregations.ReportImageAggregation;

public class ReportImage
    : Entity
{
    public string Url { get; }
    public string[] Terms { get; private set; }
    public bool IsActive { get; private set; }

    private ReportImage()
    {
    }

    public ReportImage(string url, params string[] terms)
    {
        Url = url;
        Terms = terms;
    }

    public void AddTerm(string term)
    {
        if (!Terms.Contains(term))
        {
            Terms = Terms.Append(term).ToArray();
        }
    }

    public void Activate() => IsActive = true;

    public SheetSearchResult ToSheetSearchResult()
    {
        var source = DefineFileSource();
        var serverPath = DefineServerPath(source, Url);

        var result = new SheetSearchResult(Url, source)
        {
            ServerPath = serverPath
        };

        return result;
    }

    private static string DefineServerPath(Enums.FileSource source, string url)
        => source == Enums.FileSource.Dropbox
            ? $"{AdminConstants.DropboxPath}/{url}"
            : "";

    private Enums.FileSource DefineFileSource()
    {
        Enums.FileSource source;
        if (Url.Contains("brasilsonoro"))
            source = Enums.FileSource.CrawlerDownloadLink;
        else if (!Uri.TryCreate(Url, UriKind.Absolute, out _))
            source = Enums.FileSource.Dropbox;
        else
            source = Enums.FileSource.Crawler;
        return source;
    }
}