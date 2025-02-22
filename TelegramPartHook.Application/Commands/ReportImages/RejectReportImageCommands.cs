using TelegramPartHook.Application.Factories;
using TelegramPartHook.Application.Services;
using TelegramPartHook.Domain.Aggregations.ReportImageAggregation;
using TelegramPartHook.Domain.Constants;
using TelegramPartHook.Infrastructure.Persistence;

namespace TelegramPartHook.Application.Commands.ReportImages;

public record RejectReportImageCommand
    : BaseBotStartsWithRequestCommand
{
    public const string PrefixKey = "/report-reject";
    public override string Prefix => PrefixKey;
}

public class RejectReportImageCommandHandler(
    ISearchAccessor searchAccessor,
    IAdminConfiguration adminConfiguration,
    BotContext context,
    ITelegramSender sender)
    : HandleReportImageCommandHandler<RejectReportImageCommand>(
        searchAccessor,
        adminConfiguration,
        context,
        sender)
{
    private readonly BotContext _context = context;

    protected override void HandleReportImage(ReportImage reportImage)
        => _context.Set<ReportImage>().Remove(reportImage);

    protected override string FinalMessage(ReportImage report) => $"Imagem {report.id} rejeitada";
}