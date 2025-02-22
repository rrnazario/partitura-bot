using MediatR;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types.Enums;
using TelegramPartHook.Application.DTO;
using TelegramPartHook.Application.Factories;
using TelegramPartHook.Application.Services;
using TelegramPartHook.Domain.Aggregations.ReportImageAggregation;
using TelegramPartHook.Domain.Aggregations.SearchCacheAggregation;
using TelegramPartHook.Domain.Constants;
using TelegramPartHook.Infrastructure.Persistence;

namespace TelegramPartHook.Application.Commands.ReportImages;

public record ReportImageCommand
    : BaseBotStartsWithRequestCommand
{
    public override string Prefix => "/report";
}

public record ReportImageCommandHandler(
    ISearchCacheRepository SearchCacheRepository,
    BotContext Context,
    ITelegramSender sender,
    ISearchAccessor searchAccessor)
    : IRequestHandler<ReportImageCommand>

{
    // /report term guid
    private readonly Search _search = searchAccessor.CurrentSearch();

    public async Task Handle(ReportImageCommand command, CancellationToken cancellationToken)
    {
        var (term, imageId) = ExtractInfo(command);

        var cache = await SearchCacheRepository.GetByTermAsync(term, cancellationToken);

        var reportedImage = cache?.Results.FirstOrDefault(img => img.Id == imageId);

        if (reportedImage is null)
        {
            return;
        }

        var report = await Context.Set<ReportImage>()
            .FirstOrDefaultAsync(i => i.Url == reportedImage.Address, cancellationToken);

        if (report is null)
        {
            report = new ReportImage(reportedImage.Address, term);
            Context.Add(report);
        }
        else
        {
            report.AddTerm(term);
        }

        await SearchCacheRepository.SaveChangesAsync(cancellationToken);
        await sender.SendTextMessageAsync(_search.User, Enums.MessageName.ReportedSuccessfully, cancellationToken, parseMode: ParseMode.Markdown);
        
        await sender.SendToAdminAsync("Arquivo reportado como não sendo partitura. Clique em /reviewreport", cancellationToken);
    }

    private (string term, string imageId) ExtractInfo(ReportImageCommand command)
    {
        var clearedTerm = _search.Term.Replace(command.Prefix, string.Empty).Trim().Split(' ');

        var term = string.Join(" ", clearedTerm.Take(clearedTerm.Length - 1));
        
        var id = !Guid.TryParse(clearedTerm.Last(), out var parsedId)
            ? clearedTerm.Last()
            : parsedId.ToString();

        return (term, id);
    }
}