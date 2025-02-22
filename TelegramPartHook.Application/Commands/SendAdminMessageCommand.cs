using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Telegram.Bot.Types.Enums;
using TelegramPartHook.Application.DTO;
using TelegramPartHook.Application.Factories;
using TelegramPartHook.Application.Helpers;
using TelegramPartHook.Application.Services;
using TelegramPartHook.Domain.Constants;
using TelegramPartHook.Domain.Helpers;
using TelegramPartHook.Domain.SeedWork;
using static TelegramPartHook.Domain.Constants.Enums;

namespace TelegramPartHook.Application.Commands;

public enum SendAdminMessageState
{
    Init,
    ConfirmReceived
}

public record SendAdminMessageCommand
    : BaseBotRefreshableRequest<SendAdminMessageState>,
    IRequest,
    IBotRequest
{
    public Search Search { get; set; }

    public string Prefix => string.Empty;

    public bool Match(string term)
        => term.StartsWith("/ask") || term.StartsWith("/suggestion");

    public SendAdminMessageCommand() : base(SendAdminMessageState.Init)
    {
        
    }

    public SendAdminMessageCommand(Search search) : base(SendAdminMessageState.Init)
    {
        Search = search;
    }
};

/// <summary>
/// Flow:
///     - Init: Ask for the message
///     - End: Send successful send message
/// </summary>
public class SendAdminMessageCommandHandler
    : BotInMemoryCommandHandler<SendAdminMessageState, SendAdminMessageCommand>, IRequestHandler<SendAdminMessageCommand>
{

    private const string SuggestionPlaceHolder = "/suggestion ";
    private const string AskPlaceHolder = "/ask ";

    private Search _search;

    public SendAdminMessageCommandHandler(
        IMemoryCache cache, 
        ITelegramSender sender, 
        IAdminConfiguration adminConfiguration,
        ISearchAccessor searchAccessor)
        : base(sender, cache, adminConfiguration) 
    {
        _search = searchAccessor.CurrentSearch();
    }
    
    private async Task<Unit> Init(SendAdminMessageCommand command)
    {
        if (command.Search is null)
        {
            command.Rehydrate(_search.Term);
            command.Search = _search;
        }

        command.SetNextState(SendAdminMessageState.ConfirmReceived);

        var buttons = new[] { ("Cancelar", "/cancelar") };

        var message = MessageHelper.GetMessage(command.Search.User.culture, MessageName.SendAdminMessage);

        var messageId = await Sender.SendTextMessageAsync(command.Search.User.telegramid, message, CancellationToken.None,
            ParseMode.Markdown,
            TelegramHelper.GenerateKeyboard(new(buttons)));

        command.SetLastMessageId(messageId);

        Cache.Set(command.Search.User.telegramid, command);

        return Unit.Value;
    }

    private async Task<Unit> ConfirmReceived(SendAdminMessageCommand command)
    {
        var isValid = IsValidMessage(command.Term);
        if (isValid)
        {
            var messageType = DefineMessageType(command.Term);
            var message = $"*[{messageType}]*\n\n*User:* {command.Search.User}\n\n{ClearTerm(command.Term)}";

            await Sender.SendToAdminAsync(message, CancellationToken.None);

            var successMessage = MessageHelper.GetMessage(command.Search.User.culture, MessageName.MessageSuccessfullySentToAdmin);

            await Sender.SendTextMessageAsync(command.Search.User.telegramid, successMessage, CancellationToken.None);
        }

        await ClearMemoryAsync(command.Search.User, command.LastMessageId);

        return Unit.Value;
    }

    private string ClearTerm(string originalTerm) => originalTerm.Replace(SuggestionPlaceHolder, "").Replace(AskPlaceHolder, "").Trim();

    private bool IsValidMessage(string term)
    {
        var clearedTerm = ClearTerm(term);

        return !clearedTerm.Equals("/cancelar") && !clearedTerm.Contains("/monitorar") && !clearedTerm.Contains("/suggestion");
    }

    private string DefineMessageType(string term)
    {
        return term.IndexOf(AskPlaceHolder) > 0 ? "Pedido" : "Sugestão";
    }
}