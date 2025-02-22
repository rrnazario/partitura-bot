using MediatR;
using Telegram.Bot.Types.Enums;
using TelegramPartHook.Application.DTO;
using TelegramPartHook.Application.Factories;
using TelegramPartHook.Application.Services;
using TelegramPartHook.Infrastructure.Persistence;

namespace TelegramPartHook.Application.Commands.Repertoire;

public record RemoveRepertoireSingleImageViaTelegramCommand
    : BaseBotStartsWithRequestCommand
{
    public override string Prefix => RepertoireHelper.RemoveRepertoireByImageViaTelegramCommandPrefix;
}

public class RemoveRepertoireSingleImageViaTelegramCommandHandler(
    BotContext context,
    ITelegramSender sender,
    ISearchAccessor searchAccessor)
    : IRequestHandler<RemoveRepertoireSingleImageViaTelegramCommand>

{
    // /prefix index
    private readonly Search _search = searchAccessor.CurrentSearch();

    public async Task Handle(RemoveRepertoireSingleImageViaTelegramCommand command, CancellationToken cancellationToken)
    {
        var (id, index) = ExtractInfo(command);

        var indexHasValue = int.TryParse(index, out var intIndex);
        if (string.IsNullOrEmpty(id) && !indexHasValue)
        {
            await sender.SendTextMessageAsync(_search.User.telegramid, "Ocorreu um erro ao remover uma partitura.",
                cancellationToken, ParseMode.Markdown);
            return;
        }

        var chosenSheet = string.IsNullOrEmpty(id)
            ? _search.User.Repertoire.Sheets.FirstOrDefault(s => s.Id == id)
            : _search.User.Repertoire.Sheets.Count > intIndex
                ? _search.User.Repertoire.Sheets[intIndex]
                : null;

        var keyboard = RepertoireHelper.GenerateActionKeyboard();

        if (chosenSheet is null)
        {
            await sender.SendTextMessageAsync(_search.User.telegramid,
                "Partitura já removida ou inexistente.\nExperimente enviar novamente o comando /repertorio e verificar.",
                cancellationToken, ParseMode.Markdown, keyboard: keyboard);
            return;
        }

        _search.User.Repertoire!.Remove(chosenSheet);

        context.Update(_search.User);

        await context.SaveChangesAsync(cancellationToken);

        await sender.SendTextMessageAsync(_search.User.telegramid, "Partitura removida do repertório com sucesso.",
            cancellationToken, ParseMode.Markdown, keyboard: keyboard);
    }

    private (string Id, string? index) ExtractInfo(RemoveRepertoireSingleImageViaTelegramCommand command)
    {
        var clearedTerm = _search.Term.Replace(command.Prefix, "").Trim().Split(" "); // guid id
        var id = !Guid.TryParse(clearedTerm[0], out var parsedId)
            ? clearedTerm[0]
            : parsedId.ToString();

        return clearedTerm.Length == 2 ? (id, clearedTerm[1]) : (id, "wrong");
    }
}