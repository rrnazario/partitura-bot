using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Telegram.Bot.Types.Enums;
using TelegramPartHook.Application.DTO;
using TelegramPartHook.Application.Factories;
using TelegramPartHook.Application.Helpers;
using TelegramPartHook.Application.Services;
using TelegramPartHook.Domain.Aggregations.UserAggregation;
using TelegramPartHook.Domain.Constants;
using TelegramPartHook.Domain.Helpers;
using TelegramPartHook.Domain.SeedWork;
using TelegramPartHook.Infrastructure.Persistence;
using static TelegramPartHook.Domain.Constants.Enums;

namespace TelegramPartHook.Application.Commands;

public enum RememberHandlingState
{
    Init,
    RememberSelected,
    ConfirmReceived
}

public record RememberHandlingCommand
    : BaseBotRefreshableRequest<RememberHandlingState>, 
      IBotRequest
{
    public MonitoredItem MonitoredItem { get; private set; }
    public Search Search { get; set; }

    public string Prefix => "/lembretes";
    public bool Match(string term) => term.Equals(Prefix);

    public RememberHandlingCommand() : base(RememberHandlingState.Init) { }

    public RememberHandlingCommand(Search search) : base(RememberHandlingState.Init)
    {
        Search = search;
    }

    public void SetMonitoredItem(MonitoredItem monitoredItem) => MonitoredItem = monitoredItem;

};

public class RememberHandlingCommandHandler
    : BotInMemoryCommandHandler<RememberHandlingState, RememberHandlingCommand>, 
      IRequestHandler<RememberHandlingCommand>
{
    private readonly BotContext _context;

    private const string PlaceHolder = "/remover ";

    private Search _search;

    public RememberHandlingCommandHandler(
        IMemoryCache cache, 
        ITelegramSender sender, 
        BotContext context, 
        IAdminConfiguration adminConfiguration,
        ISearchAccessor searchAccessor)
        : base(sender, cache, adminConfiguration)
    {
        _context = context;

        _search = searchAccessor.CurrentSearch();
    }

    private async Task<Unit> Init(RememberHandlingCommand command)
    {
        if (command.Search is null)
        {
            command.Rehydrate(_search.Term);
            command.Search = _search;
        }

        command.SetNextState(RememberHandlingState.RememberSelected);

        var buttons = command.Search.User
            .GetMonitoredItems()
            .Select((item, i) => (item.Format(), $"{PlaceHolder}{i}"))
            .ToList();

        if (!buttons.Any())
        {
            var notFoundMessage = MessageHelper.GetMessage(command.Search.User.culture, MessageName.ThereAreNoRemembers);

            await Sender.SendTextMessageAsync(command.Search.User.telegramid, notFoundMessage, CancellationToken.None, ParseMode.Markdown);

            await ClearMemoryAsync(command.Search.User, command.LastMessageId, false);

            return Unit.Value;
        }

        buttons.Add(("Cancelar", "/cancelar"));

        var message = MessageHelper.GetMessage(command.Search.User.culture, MessageName.ChoseRemember);

        var messageId = await Sender.SendTextMessageAsync(command.Search.User.telegramid, message, CancellationToken.None,
            ParseMode.Markdown,
            TelegramHelper.GenerateKeyboard(new KeyboardButtons(buttons)));

        command.SetLastMessageId(messageId);

        Cache.Set(command.Search.User.telegramid, command);

        return Unit.Value;
    }

    private async Task<Unit> RememberSelected(RememberHandlingCommand command)
    {
        if (int.TryParse(command.Term.Replace(PlaceHolder, ""), out var chosenItemIndex))
        {
            command.SetMonitoredItem(command.Search.User.GetMonitoredItems()[chosenItemIndex]);
            command.SetNextState(RememberHandlingState.ConfirmReceived);

            var message = MessageHelper.GetMessage(command.Search.User.culture, MessageName.ConfirmRememberExclusion, command.MonitoredItem.Format());

            var messageId = await Sender.EditMessageTextAsync(command.Search.User.telegramid, command.LastMessageId, message, CancellationToken.None,
                            ParseMode.Markdown,
                            TelegramHelper.GenerateTrueFalseKeyboard(PlaceHolder));

            command.SetLastMessageId(messageId);

            Cache.Set(command.Search.User.telegramid, command);
        }
        else
        {
            await ClearMemoryAsync(command.Search.User, command.LastMessageId);
        }

        return Unit.Value;
    }

    private async Task<Unit> ConfirmReceived(RememberHandlingCommand command)
    {
        if (bool.TryParse(command.Term.Replace(PlaceHolder, ""), out var confirmed) && confirmed)
        {
            command.Search.User.RemoveScheduledSearch(command.MonitoredItem);
            _context.Update(command.Search.User);

            _context.SaveChanges();

            var message = MessageHelper.GetMessage(command.Search.User.culture, MessageName.RememberSuccessfullyRemoved, command.MonitoredItem.Format());

            await Sender.EditMessageTextAsync(command.Search.User.telegramid, command.LastMessageId,
                message, CancellationToken.None);
        }

        await ClearMemoryAsync(command.Search.User, command.LastMessageId, !confirmed);

        return Unit.Value;
    }
}