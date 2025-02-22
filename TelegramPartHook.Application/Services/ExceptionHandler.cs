using MediatR;
using Serilog;
using Telegram.Bot.Types;
using TelegramPartHook.Application.Factories;
using TelegramPartHook.Application.Helpers;
using TelegramPartHook.Application.Notifications;
using TelegramPartHook.Domain.Exceptions;
using TelegramPartHook.Domain.Helpers;
using TelegramPartHook.Domain.SeedWork;

namespace TelegramPartHook.Application.Services;

public interface IExceptionHandler
{
    Task HandleAsync(Exception e);
}

public class ExceptionHandler
    : IExceptionHandler
{
    private readonly ILogHelper _logHelper;
    private readonly ITelegramSender _sender;
    private readonly ISearchFactory _searchFactory;
    private readonly IMediator _mediator;

    public ExceptionHandler(ILogHelper log, ITelegramSender sender, ISearchFactory searchFactory, IMediator mediator)
    {
        _logHelper = log;
        _sender = sender;
        _searchFactory = searchFactory;
        _mediator = mediator;
    }

    public async Task HandleAsync(Exception e)
    {
        Log.Error(e.Message);

        if (e is IgnoreNonAdminException)
        {
            return;
        }

        var sendToAdmin = true;

        if (e is IPartBotCustomException pbe)
        {
            sendToAdmin = await HandleBotExceptionsAsync(pbe);
        }

        if (sendToAdmin)
        {
            await _mediator.Publish(new SaveErrorEvent(e));
            await SendToAdminAsync(e);
        }
    }

    private async Task SendToAdminAsync(Exception e)
    {
        var search = _searchFactory.GetCurrentSearch();
        await _logHelper.ErrorAsync(e, CancellationToken.None, search: search);

        if (e.InnerException is not null)
            await _logHelper.ErrorAsync(e.InnerException, CancellationToken.None, search: search);
    }

    private async Task<bool> HandleBotExceptionsAsync(IPartBotCustomException pbe)
    {
        if (pbe is NotVipUserException nve)
        {
            return await HandleNotVipExceptionAsync(nve);
        }

        var parameters = pbe is IPartBotParamException pbpe
            ? pbpe.Parameters
            : [];

        var message = MessageHelper.GetMessage(pbe.User.culture, pbe.Message, parameters);

        await _sender.SendTextMessageAsync(pbe.User.telegramid, message, CancellationToken.None);

        return pbe.SendToAdmin;
    }

    private async Task<bool> HandleNotVipExceptionAsync(NotVipUserException nve)
    {
        var caption = string.IsNullOrWhiteSpace(nve.CustomMessage)
            ? MessageHelper.GetMessage(nve.User.culture, nve.Message, nve.Parameters)
            : MessageHelper.GetMessage(nve.CustomMessage, nve.Parameters);


        var pixInfo = PixHelper.GeneratePixString(nve.User);
        var qrCodeFile = PixHelper.GenerateQrCodeImage(pixInfo);

        await using var stream = new FileStream(qrCodeFile, FileMode.Open, FileAccess.Read);

        await _sender.SendPhotoAsync(nve.User.telegramid, InputFile.FromStream(stream),
            caption: caption,
            cancellationToken: CancellationToken.None);

        await _sender.SendTextMessageAsync(nve.User.telegramid, pixInfo, CancellationToken.None);

        try
        {
            if (System.IO.File.Exists(qrCodeFile))
                System.IO.File.Delete(qrCodeFile);
        }
        catch
        {
            // ignored
        }

        return false;
    }
}