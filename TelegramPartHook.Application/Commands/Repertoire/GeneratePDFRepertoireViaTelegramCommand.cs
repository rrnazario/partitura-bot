using Light.GuardClauses;
using MediatR;
using TelegramPartHook.Application.DTO;
using TelegramPartHook.Application.Factories;
using TelegramPartHook.Application.Services;
using TelegramPartHook.Domain.Exceptions;
using TelegramPartHook.Domain.SeedWork;

namespace TelegramPartHook.Application.Commands.Repertoire;

public record GeneratePDFRepertoireViaTelegramCommand
     : BaseBotRequestCommand
{
    public override string Prefix => RepertoireHelper.GeneratePDFRepertoireViaTelegramCommandPrefix;
}

public class GeneratePDFRepertoireViaTelegramCommandHandler
    : IRequestHandler<GeneratePDFRepertoireViaTelegramCommand>
{
    private readonly ITelegramSender _sender;
    private Search _search;
    private readonly IPdfService _pdfService;

    public GeneratePDFRepertoireViaTelegramCommandHandler(
        ITelegramSender sender,
        ISearchAccessor searchAccessor,
        IPdfService pdfService)
    {
        _search = searchAccessor.CurrentSearch();
        if (!_search.User.IsVipValid())
        {
            throw new NotVipUserException(_search.User);
        }

        _sender = sender.MustNotBeNull();

        _pdfService = pdfService;
    }

    public async Task Handle(GeneratePDFRepertoireViaTelegramCommand request, CancellationToken cancellationToken)
    {
        var keyboard = RepertoireHelper.GenerateActionKeyboard();

        if (!_search.User.Repertoire.Sheets.Any())
        {
            await _sender.SendTextMessageAsync(_search.User.telegramid, "Não há itens no seu repertório.", cancellationToken, keyboard: keyboard);
            return;
        }

        var pdfPath = await _pdfService.GenerateAsync([.._search.User.Repertoire.Sheets], $"Repertorio {DateTime.Now:dd-MM-yyyy HH-mm-ss}");

        var file = SheetSearchResult.CreateLocalFile(pdfPath);

        await _sender.SendFilesAsync([file], _search.User, false, cancellationToken);

        await _sender.SendTextMessageAsync(_search.User.telegramid, "Ações para o repertório:", cancellationToken, keyboard: keyboard);
    }
}
