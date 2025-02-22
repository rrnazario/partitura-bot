using MediatR;
using Microsoft.Extensions.Caching.Memory;
using TelegramPartHook.Application.DTO;
using TelegramPartHook.Application.Factories;
using TelegramPartHook.Application.Helpers;
using TelegramPartHook.Application.Services;
using TelegramPartHook.Domain.Constants;
using TelegramPartHook.Domain.SeedWork;
using TelegramPartHook.Infrastructure.Persistence;

namespace TelegramPartHook.Application.Commands.Repertoire;

public enum CleanRepertoireHandlingState
{
    Init,
    ConfirmReceived
}

public record CleanRepertoireViaTelegramCommand
    : BaseBotRefreshableRequest<CleanRepertoireHandlingState>
        , IBotRequest
{
    public string Prefix => RepertoireHelper.CleanRepertoireViaTelegramCommandPrefix;

    public CleanRepertoireViaTelegramCommand() : base(CleanRepertoireHandlingState.Init)
    {
    }

    public bool Match(string term) => term.StartsWith(Prefix, StringComparison.InvariantCultureIgnoreCase);
}

public class CleanRepertoireViaTelegramCommandHandler
    : BotInMemoryCommandHandler<CleanRepertoireHandlingState, CleanRepertoireViaTelegramCommand>
        , IRequestHandler<CleanRepertoireViaTelegramCommand>
{
    private Search _search;
    private readonly BotContext _context;

    public CleanRepertoireViaTelegramCommandHandler(
        ITelegramSender sender,
        IMemoryCache cache,
        ISearchAccessor searchAccessor,
        BotContext context,
        IAdminConfiguration adminConfiguration)
        : base(sender, cache, adminConfiguration)
    {
        _search = searchAccessor.CurrentSearch();
        _context = context;
    }

    private async Task<Unit> Init(CleanRepertoireViaTelegramCommand command)
    {
        command.SetNextState(CleanRepertoireHandlingState.ConfirmReceived);

        var keyboard = TelegramHelper.GenerateTrueFalseKeyboard($"{command.Prefix} ");
        var lastMessageId = Sender.SendTextMessageAsync(_search.User.telegramid,
                $"Deseja confirmar a limpeza do repertório?", CancellationToken.None, keyboard: keyboard).GetAwaiter()
            .GetResult();

        command.SetLastMessageId(lastMessageId);

        Cache.Set(_search.User.telegramid, command);

        return Unit.Value;
    }

    private async Task ConfirmReceived(CleanRepertoireViaTelegramCommand command)
    {
        var keyboard = RepertoireHelper.GenerateActionKeyboard();

        if (bool.TryParse(command.ClearTerm(command.Prefix), out var confirmed) && confirmed)
        {
            if (_search.User.Repertoire.Sheets.Any())
            {
                _search.User.Repertoire.Clear();
                _context.Update(_search.User);

                await _context.SaveChangesAsync();
            }

            await Sender.EditMessageTextAsync(_search.User.telegramid, command.LastMessageId,
                "Repertório limpo com sucesso.", CancellationToken.None, keyboard: keyboard);

            await ClearMemoryAsync(_search.User, command.LastMessageId, false);
        }
        else
        {
            await ClearMemoryAsync(_search.User, command.LastMessageId, keyboard: keyboard);
        }
    }
}