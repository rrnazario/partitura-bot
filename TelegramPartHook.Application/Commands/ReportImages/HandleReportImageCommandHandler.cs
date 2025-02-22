using Microsoft.EntityFrameworkCore;
using TelegramPartHook.Application.Factories;
using TelegramPartHook.Application.Services;
using TelegramPartHook.Domain.Aggregations.ReportImageAggregation;
using TelegramPartHook.Domain.Constants;
using TelegramPartHook.Infrastructure.Persistence;

namespace TelegramPartHook.Application.Commands.ReportImages;

public abstract class HandleReportImageCommandHandler<T>(
    ISearchAccessor searchAccessor,
    IAdminConfiguration adminConfiguration,
    BotContext context,
    ITelegramSender sender)
    : BaseAdminBotRequestCommandHandler<T>(searchAccessor, adminConfiguration)
    where T: BaseBotStartsWithRequestCommand
{
    public override async Task Handle(T request, CancellationToken cancellationToken)
    {
        var imageId = Search.Term.Replace(request.Prefix, string.Empty).Trim();

        if (!int.TryParse(imageId, out var parsedImageId))
        {
            await sender.SendToAdminAsync("Erro ao identificar a imagem.", cancellationToken);
            return;
        }
        
        var report = await context.Set<ReportImage>()
            .FirstOrDefaultAsync(f => f.id == parsedImageId, cancellationToken);

        if (report is null)
        {
            await sender.SendToAdminAsync("Imagem não encontrada.", cancellationToken);
            return;
        }

        HandleReportImage(report);
        await context.SaveChangesAsync(cancellationToken);
        
        await sender.SendToAdminAsync(FinalMessage(report), cancellationToken);
    }

    protected abstract void HandleReportImage(ReportImage reportImage);
    protected abstract string FinalMessage(ReportImage reportImage);
}