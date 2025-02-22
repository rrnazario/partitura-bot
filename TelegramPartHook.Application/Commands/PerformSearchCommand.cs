using MediatR;
using Telegram.Bot.Types.Enums;
using TelegramPartHook.Application.Commands.Repertoire;
using TelegramPartHook.Application.Helpers;
using TelegramPartHook.Application.Services;
using TelegramPartHook.Application.DTO;
using TelegramPartHook.Domain.SeedWork;
using static TelegramPartHook.Domain.Constants.Enums;
using TelegramPartHook.Application.Queries;
using TelegramPartHook.Infrastructure.Persistence;
using TelegramPartHook.Application.Factories;
using TelegramPartHook.Domain.Helpers;
using IBaseRequest = TelegramPartHook.Domain.SeedWork.IBaseRequest;

namespace TelegramPartHook.Application.Commands;

public record PerformSearchCommand : IBaseRequest, IRequest;

public class PerformSearchCommandHandler
    : IRequestHandler<PerformSearchCommand>
{
    private readonly IMediator _mediator;
    private readonly IGlobalState _globalState;
    private readonly ITelegramSender _sender;
    private readonly IUnitOfWork _unitOfWork;
    private readonly BotContext _context;
    private readonly ILogHelper _log;
    private readonly Search _search;

    public PerformSearchCommandHandler(IGlobalState globalState,
        ITelegramSender sender,
        IMediator mediator,
        ILogHelper log,
        BotContext context,
        IUnitOfWork unitOfWork,
        ISearchAccessor searchAccessor)
    {
        _globalState = globalState;
        _sender = sender;
        _mediator = mediator;
        _log = log;
        _context = context;
        _unitOfWork = unitOfWork;

        _search = searchAccessor.CurrentSearch();
    }

    public async Task Handle(PerformSearchCommand request, CancellationToken cancellationToken)
    {
        if (_globalState.IsDuplicateSearch(_search.User.telegramid, _search.Term))
            return;

        _globalState.MarkGlobalSearching(_search.User.telegramid, _search.Term);

        try
        {
            await SendSearchingMessageAsync(cancellationToken);

            var parts = (await _mediator.Send(new GetSheetLinksQuery(_search.Term), cancellationToken))
                .ToArray();

            if (parts.Length == 0)
            {
                await SendNotFoundMessageAsync(cancellationToken);
            }
            else
            {
                await SendFilesAsync(parts, cancellationToken);
                await SendSuccessMessageAsync(parts.Length, cancellationToken);
            }

            await UpdateSearchAsync(parts, cancellationToken);
        }
        finally
        {
            _globalState.MarkGlobalStopSearching(_search.User.telegramid, _search.Term);
        }
    }

    private async Task SendFilesAsync(SheetSearchResult[] parts, CancellationToken cancellationToken)
    {
        _globalState.MarkGlobalSending(_search.User.telegramid);

        SetKeyboardPerFile(parts);
        
        var counter =
            await _sender.SendFilesAsync(parts.Distinct(), _search.User, cancellationToken: cancellationToken);

        _log.Info($"Total sent: {counter - 1}", cancellationToken);

        _globalState.MarkGlobalStopSending(_search.User.telegramid);

        _globalState.CheckCleanFolders();
    }

    private void SetKeyboardPerFile(SheetSearchResult[] parts)
    {
        for (var index = 0; index < parts.Length; index++)
        {
            var part = parts[index];

            var buttons = new KeyboardButtons()
                .Add((MessageHelper.GetMessage(_search.User.culture, MessageName.AddRepertoireViaTelegram), $"{RepertoireHelper.AddRepertoireByImageViaTelegramCommandPrefix} {_search.Term} {index}"))
                .Add((MessageHelper.GetMessage(_search.User.culture, MessageName.Report), $"/report {_search.Term.Trim()} {part.Id}"));
            
            part.SetButtons(buttons);
        }
    }

    private async Task UpdateSearchAsync(SheetSearchResult[] parts, CancellationToken cancellationToken)
    {
        _search.User.UpdateSearchState(_search.Term, parts);
        _context.Update(_search.User);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task SendSearchingMessageAsync(CancellationToken cancellationToken)
    {
        if (_search.User.IsCallback)
            await _sender.SendTextMessageAsync(_search.User, MessageName.SearchingSuggestion, cancellationToken,
                placeholders: [_search.Term]);
        else
            await _sender.SendTextMessageAsync(_search.User.telegramid,
                MessageHelper.GetRandomSearchingMessage(_search.User.culture), cancellationToken);
    }

    private async Task SendSuccessMessageAsync(int searchTotal, CancellationToken cancellationToken)
    {
        var successMessage = MessageHelper.GetRandomSuccessMessage(_search.User.culture);
        var buttons = new List<(string caption, string url)>
        {
            (MessageHelper.GetMessage(_search.User.culture, MessageName.DownloadAsPDF), $"/pdf {_search.Term}"),
            (successMessage.buttonCaption, successMessage.buttonUrl),
            (MessageHelper.GetMessage(_search.User.culture, MessageName.Contribute),
                $"https://partituravip.com.br/contribute?user={(_search.User.IsVipValid() ? _search.User.GetPortalUsername() : _search.User.telegramid)}"),
        };

        if (!_search.User.IsVipValid())
            buttons.Add((MessageHelper.GetMessage(_search.User.culture, MessageName.MakeMeVIP), $"/vip"));

        await _sender.SendTextMessageAsync(_search.User.telegramid, successMessage.text, cancellationToken,
            ParseMode.Markdown,
            TelegramHelper.GenerateKeyboard(new KeyboardButtons(buttons)));
    }

    private async Task SendNotFoundMessageAsync(CancellationToken cancellationToken)
    {
        var msg = MessageHelper.GetRandomNotFoundMessage(_search.User.fullname, _search.User.culture);
        var url = $"/monitorar {_search.Term}";

        var buttons = new List<(string text, string url)>
        {
            (MessageHelper.GetMessage(_search.User.culture, MessageName.MonitorForMe), url),
            (MessageHelper.GetMessage(_search.User.culture, MessageName.Contribute),
                $"https://partituravip.com.br/contribute?user={(_search.User.IsVipValid() ? _search.User.GetPortalUsername() : _search.User.telegramid)}")
        };

        if (_search.User.IsVipValid())
            buttons.Add((MessageHelper.GetMessage(_search.User.culture, MessageName.SendSuggestion), "/suggestion"));

        await _sender.SendTextMessageAsync(_search.User.telegramid, msg, cancellationToken,
            keyboard: TelegramHelper.GenerateKeyboard(new KeyboardButtons(buttons)));
    }
}