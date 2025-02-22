using Serilog;
using System.Runtime.CompilerServices;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;
using TelegramPartHook.Application.DTO;
using TelegramPartHook.Application.Services;

namespace TelegramPartHook.Application.Helpers;

public interface ILogHelper
{
    void Info(string message, CancellationToken cancellationToken, bool sendToAdmin = false,
        [CallerMemberName] string callerName = "");
    void Info(Exception e, CancellationToken cancellationToken, bool sendToAdmin = false, [CallerMemberName] string callerName = "");
    Task ErrorAsync(Exception e, CancellationToken cancellationToken, [CallerMemberName] string callerName = "", Search? search = null);
    Task<bool> SendMessageToAdminAsync(string message, CancellationToken cancellationToken);
}

public class LogHelper : ILogHelper
{
    private readonly ITelegramSender _sender;

    public LogHelper(ITelegramSender sender)
    {
        _sender = sender;
    }

    public void Info(string message, CancellationToken cancellationToken, bool sendToAdmin = false,
        [CallerMemberName] string callerName = "")
    {
        message = $"[{callerName}]\n{message}";

        Log.Information(message);

        if (sendToAdmin)
            SendMessageToAdminAsync(message, cancellationToken).GetAwaiter().GetResult();
    }

    public void Info(Exception e, CancellationToken cancellationToken, bool sendToAdmin = false, [CallerMemberName] string callerName = "")
        => Info(
            $"{e.Message}\n\n{e.StackTrace}" + (e.InnerException != null
                ? $"\nINNER:\n{e.InnerException.Message}\n\n{e.InnerException.StackTrace}"
                : string.Empty), cancellationToken, sendToAdmin, callerName);

    public async Task ErrorAsync(Exception e, CancellationToken cancellationToken, [CallerMemberName] string callerName = "", Search? search = null)
    {
        var messageMd = MountErrorMessage(e, callerName, search);

        try
        {
            var success = await SendMessageToAdminAsync(messageMd, cancellationToken);

            if (!success)
            {
                messageMd = SimplifyErrorMessage(callerName, e, search);

                await SendMessageToAdminAsync(messageMd, cancellationToken);
            }
        }
        catch
        {
            messageMd = SimplifyErrorMessage(callerName, e, search);

            await SendMessageToAdminAsync(messageMd, cancellationToken);
        }
    }

    private static string SimplifyErrorMessage(string caller, Exception e, Search? search)
        => $"Metodo: _{caller}_ *\n\n{e.Message}*\n\n{e.InnerException?.Message ?? "No inner exception"}{MountUserInfo(search)}";

    private static string MountErrorMessage(Exception e, string callerName, Search? search)
    {
        var messageMD = $"Metodo: _{callerName}_ *\n\n{e.Message}*";

        if (search is not null)
        {
            messageMD += MountUserInfo(search);
        }

        messageMD += $"\n\n{e.StackTrace}";

        return messageMD;
    }

    private static string MountUserInfo(Search? search) => $"\n\nUser: {(search?.User?.ToString() ?? "Usuario não obtido")}, Search: {(search?.Term ?? "Termo não obtido")}";

    public async Task<bool> SendMessageToAdminAsync(string message, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _sender.SendToAdminAsync(message, cancellationToken, ParseMode.Markdown);

            return result != TelegramSenderMessageResult.AdminMessageNotSent;
        }
        catch (ApiRequestException aex)
        {
            if (aex.Message.Contains("Can't find end of the entity starting at byte offset",
                    StringComparison.InvariantCultureIgnoreCase))
                await _sender.SendToAdminAsync(message, cancellationToken);
        }

        return false;
    }
}