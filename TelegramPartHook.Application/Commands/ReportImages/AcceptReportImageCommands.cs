using TelegramPartHook.Application.Factories;
using TelegramPartHook.Application.Services;
using TelegramPartHook.Domain.Aggregations.ReportImageAggregation;
using TelegramPartHook.Domain.Constants;
using TelegramPartHook.Infrastructure.Persistence;

namespace TelegramPartHook.Application.Commands.ReportImages;

public record AcceptReportImageCommand
    : BaseBotStartsWithRequestCommand
{
    public const string PrefixKey = "/report-accept";
    public override string Prefix => PrefixKey;
}

public class AcceptReportImageCommandHandler(
    ISearchAccessor searchAccessor,
    IAdminConfiguration adminConfiguration,
    BotContext context,
    ITelegramSender sender)
    : HandleReportImageCommandHandler<AcceptReportImageCommand>(searchAccessor, adminConfiguration, context, sender)
{
    protected override void HandleReportImage(ReportImage reportImage)
        => reportImage.Activate();

    protected override string FinalMessage(ReportImage report) => $"Imagem {report.id} aceita com sucesso";
}