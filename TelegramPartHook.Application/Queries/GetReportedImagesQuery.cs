using Microsoft.EntityFrameworkCore;
using TelegramPartHook.Application.Commands;
using TelegramPartHook.Application.Commands.ReportImages;
using TelegramPartHook.Application.Factories;
using TelegramPartHook.Application.Services;
using TelegramPartHook.Domain.Aggregations.ReportImageAggregation;
using TelegramPartHook.Domain.Constants;
using TelegramPartHook.Domain.SeedWork;
using TelegramPartHook.Infrastructure.Persistence;

namespace TelegramPartHook.Application.Queries;

public record GetReportedImagesQuery
    : BaseBotRequestCommand
{
    public override string Prefix => "/reviewreport";
}

public class CheckReportedImagesCommandHandler(
    ISearchAccessor searchAccessor,
    IAdminConfiguration adminConfiguration,
    BotContext context,
    ITelegramSender sender)
    : BaseAdminBotRequestCommandHandler<GetReportedImagesQuery>(
        searchAccessor, adminConfiguration)
{
    public override async Task Handle(GetReportedImagesQuery request, CancellationToken cancellationToken)
    {
        var reports = await context.Set<ReportImage>()
            .AsNoTracking()
            .Where(i => !i.IsActive)
            .ToListAsync(cancellationToken);

        var sheets = GenerateSheets(reports);

        await sender.SendFilesAsync(sheets, Search.User, verifyUserReceiving: false,
            cancellationToken: cancellationToken);
    }

    private static List<SheetSearchResult> GenerateSheets(List<ReportImage> reports)
    {
        var searches = new List<SheetSearchResult>();
        foreach (var report in reports)
        {
            var buttons = new KeyboardButtons()
                .Add(("Aceitar", $"{AcceptReportImageCommand.PrefixKey} {report.id}"))
                .Add(("Rejeitar", $"{RejectReportImageCommand.PrefixKey} {report.id}"));

            var search = report.ToSheetSearchResult();
            search.SetButtons(buttons);
            search.SetCaption(string.Join(", ", report.Terms));

            searches.Add(search);
        }

        return searches;
    }
}