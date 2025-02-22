using Light.GuardClauses;
using MediatR;
using TelegramPartHook.Application.DTO;
using TelegramPartHook.Application.Factories;
using TelegramPartHook.Application.Services;
using TelegramPartHook.Domain.SeedWork;

namespace TelegramPartHook.Application.Commands.Repertoire;

public record SeeRepertoireViaTelegramCommand
    : BaseBotRequestCommand
{
    public override string Prefix => RepertoireHelper.SeeRepertoireViaTelegramCommandPrefix;
}

public class SeeRepertoireViaTelegramCommandHandler
    : IRequestHandler<SeeRepertoireViaTelegramCommand>
{
    private readonly ITelegramSender _sender;
    private Search _search;

    public SeeRepertoireViaTelegramCommandHandler(
        ITelegramSender sender,
        ISearchAccessor searchAccessor)
    {
        _sender = sender.MustNotBeNull();

        _search = searchAccessor.CurrentSearch();
    }

    public async Task Handle(SeeRepertoireViaTelegramCommand request, CancellationToken cancellationToken)
    {
        if (_search.User.Repertoire is null or { Sheets.Count: 0 })
        {
            await _sender.SendTextMessageAsync(_search.User.telegramid, "Não há itens no seu repertório",
                cancellationToken);
            return;
        }

        var sheets = _search.User.Repertoire.Sheets.ToArray();

        SetKeyboardPerFile(sheets);

        await _sender.SendFilesAsync(sheets, _search.User, false, cancellationToken);

        var keyboard = RepertoireHelper.GenerateActionKeyboard();

        await _sender.SendTextMessageAsync(_search.User.telegramid, "Ações para o repertório", cancellationToken,
            keyboard: keyboard);
    }
    
    private static void SetKeyboardPerFile(SheetSearchResult[] parts)
    {
        for (var index = 0; index < parts.Length; index++)
        {
            var part = parts[index];

            var buttons = new KeyboardButtons()
                .Add(("Remover item", $"{RepertoireHelper.RemoveRepertoireByImageViaTelegramCommandPrefix} {part.Id} {index}"));
            
            part.SetButtons(buttons);
        }
    }
}