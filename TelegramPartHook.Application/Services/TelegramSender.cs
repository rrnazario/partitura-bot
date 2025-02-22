using MediatR;
using Serilog;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramPartHook.Application.Helpers;
using TelegramPartHook.Application.Interfaces.Searches;
using TelegramPartHook.Application.Notifications;
using TelegramPartHook.Domain.Constants;
using TelegramPartHook.Domain.Exceptions;
using TelegramPartHook.Domain.Helpers;
using TelegramPartHook.Domain.SeedWork;
using TelegramPartHook.Infrastructure.Helpers;
using static TelegramPartHook.Domain.Constants.Enums;
using File = System.IO.File;
using User = TelegramPartHook.Domain.Aggregations.UserAggregation.User;

namespace TelegramPartHook.Application.Services;

public enum TelegramSenderMessageResult
{
    Default,
    AdminMessageNotSent
}

public interface ITelegramSender
{
    Task<int> EditMessageTextAsync(string chatId, int messageId, string message, CancellationToken cancellationToken,
        ParseMode parseMode = ParseMode.MarkdownV2, InlineKeyboardMarkup? keyboard = null);

    Task<int> SendTextMessageAsync(string chatId, string message, CancellationToken cancellationToken,
        ParseMode parseMode = ParseMode.MarkdownV2, InlineKeyboardMarkup? keyboard = null);

    Task SendTextMessageAsync(User user, MessageName message, CancellationToken cancellationToken,
        string[]? placeholders = null, ParseMode parseMode = ParseMode.MarkdownV2,
        InlineKeyboardMarkup? keyboard = null);

    Task<TelegramSenderMessageResult> SendToAdminAsync(string message, CancellationToken cancellationToken,
        ParseMode parseMode = ParseMode.MarkdownV2, InlineKeyboardMarkup? keyboard = null);

    Task SendPhotoAsync(string chatId, InputFile inputOnlineFile, CancellationToken cancellationToken,
        string caption = "", InlineKeyboardMarkup? keyboard = null);

    Task SendFileAsync(string chatId, InputFile inputOnlineFile, CancellationToken cancellationToken,
        string caption = "", InlineKeyboardMarkup? keyboard = null);

    Task<int> SendFilesAsync(IEnumerable<SheetSearchResult> sheets, User user, bool verifyUserReceiving = true,
        CancellationToken cancellationToken = default);
}

public class TelegramSender : ITelegramSender
{
    private readonly ITelegramBotClient _client;
    private readonly IGlobalState _globalState;
    private readonly IAdminConfiguration _adminConfiguration;
    private readonly IDropboxService _dropboxService;
    private readonly ISystemHelper _systemHelper;
    private readonly IMediator _mediator;

    public TelegramSender(IGlobalState globalState,
        IAdminConfiguration adminConfiguration,
        IDropboxService dropboxService,
        IMediator mediator,
        ISystemHelper systemHelper)
    {
        _client = new TelegramBotClient(adminConfiguration.TelegramBotToken);
        _globalState = globalState;
        _adminConfiguration = adminConfiguration;
        _dropboxService = dropboxService;
        _mediator = mediator;
        _systemHelper = systemHelper;
    }

    public async Task<int> SendTextMessageAsync(string chatId, string message, CancellationToken cancellationToken,
        ParseMode parseMode = ParseMode.MarkdownV2, InlineKeyboardMarkup? keyboard = null)
    {
        try
        {
            message = ThreatMessage(message, parseMode);

            var messageResult = await _client.SendTextMessageAsync(chatId, message, parseMode: parseMode,
                replyMarkup: keyboard, cancellationToken: cancellationToken);

            return messageResult.MessageId;
        }
        catch (Exception exc) when (exc.Message.Contains("Forbidden"))
        {
            Log.Error($"User {chatId} blocked the bot");

            await RemoveDeactivatedUser(chatId, cancellationToken);
        }
        catch (Exception exc) when (exc.Message.Contains("chat not found"))
        {
            Log.Error($"Chat {chatId} not found.");

            await RemoveDeactivatedUser(chatId, cancellationToken);
        }
        catch (Exception exc)
        {
            Log.Error(exc.Message);

            if (chatId != _adminConfiguration.AdminChatId)
            {
                var msg = new StringBuilder($"*CHAT ID: {chatId}\n\nMESSAGE: {message}");

                if (keyboard is not null)
                {
                    msg.Append("\n\nButtons:");
                    keyboard.InlineKeyboard.ToList()
                        .ForEach(b => msg.Append(b.Select(s => $"\n{s.Text} ::: {s.CallbackData}")));
                }

                msg.Append($"\n\n{exc.Message}*");

                await SendToAdminAsync(msg.ToString(), cancellationToken);
            }
            else
            {
                return (int)TelegramSenderMessageResult.AdminMessageNotSent;
            }
        }

        return default;
    }

    private async Task RemoveDeactivatedUser(string chatId, CancellationToken cancellation)
    {
        await _mediator.Publish(new RemoveDeactivatedUserEvent(chatId), cancellation);
    }

    public async Task SendTextMessageAsync(User user, MessageName message, CancellationToken cancellationToken,
        string[]? placeholders = null, ParseMode parseMode = ParseMode.MarkdownV2,
        InlineKeyboardMarkup? keyboard = null)
        => await SendTextMessageAsync(user.telegramid,
            MessageHelper.GetMessage(user.culture, message, placeholders ?? Array.Empty<string>()), cancellationToken,
            parseMode, keyboard);

    public async Task<TelegramSenderMessageResult> SendToAdminAsync(string message, CancellationToken cancellationToken,
        ParseMode parseMode = ParseMode.MarkdownV2, InlineKeyboardMarkup? keyboard = null)
        => (TelegramSenderMessageResult)await SendTextMessageAsync(_adminConfiguration.AdminChatId, message,
            cancellationToken, parseMode, keyboard);
    
    public async Task SendFileAsync(string chatId, InputFile inputOnlineFile, CancellationToken cancellationToken,
        string caption = "", InlineKeyboardMarkup? keyboard = null)
        => await _client.SendDocumentAsync(chatId, inputOnlineFile, cancellationToken: cancellationToken, caption: caption,
            replyMarkup: keyboard);

    public async Task SendPhotoAsync(string chatId, InputFile inputOnlineFile, CancellationToken cancellationToken,
        string caption = "", InlineKeyboardMarkup? keyboard = null)
        => await _client.SendPhotoAsync(chatId, inputOnlineFile, cancellationToken: cancellationToken, caption: caption,
            replyMarkup: keyboard);

    public async Task<int> EditMessageTextAsync(string chatId, int messageId, string message,
        CancellationToken cancellationToken, ParseMode parseMode = ParseMode.MarkdownV2,
        InlineKeyboardMarkup? keyboard = null)
    {
        try
        {
            message = ThreatMessage(message, parseMode);

            var messageResult = await _client.EditMessageTextAsync(new ChatId(long.Parse(chatId)), messageId, message,
                parseMode, replyMarkup: keyboard ?? InlineKeyboardMarkup.Empty());

            if (keyboard is null)
                await _client.EditMessageReplyMarkupAsync(new ChatId(long.Parse(chatId)), messageId,
                    replyMarkup: InlineKeyboardMarkup.Empty(), cancellationToken);

            return messageResult.MessageId;
        }
        catch (Exception exc)
        {
            await SendToAdminAsync($"*{exc.Message}*\n\n{exc.StackTrace}", cancellationToken);
        }

        return default;
    }

    private static string ThreatMessage(string message, ParseMode parseMode)
    {
        if (message.Length >= 4096)
            message = message[..4090];

        if (parseMode == ParseMode.MarkdownV2)
        {
            new[] { "-", ".", "(", ")", "!", "+", "=", "{", "}", "|" }
                .ToList()
                .ForEach(c => message = message.Replace(c, $"\\{c}"));
        }

        return message;
    }

    public async Task<int> SendFilesAsync(IEnumerable<SheetSearchResult> sheets, User user,
        bool verifyUserReceiving = true, CancellationToken cancellationToken = default)
    {
        int count = 1, total = sheets.Count();

        if (verifyUserReceiving)
            await SendWarningManyMessages(user, total, cancellationToken);

        foreach (var part in sheets)
        {
            Log.Logger.Information($"[{count} de {total}] Enviando: {part}");

            try
            {
                await Task.WhenAll(DownloadContentIfNeededAsync(part, user.SearchFolder),
                    Task.Delay(TimeSpan.FromSeconds(count == 1 ? 0 : 1), cancellationToken));

                //To prevent 429 error, Telegram FAQ says to send 1 info per second (at least).
            }
            catch (NotSheetException)
            {
                Log.Logger.Warning($"{part} is not a score sheet.");
                if (part.Source == FileSource.Dropbox)
                {
                    await _dropboxService.DeleteFileAsync(part.ServerPath);
                }

                continue;
            }
            catch (Exception e)
            {
                Log.Logger.Error(e.Message);

                continue;
            }

            if (verifyUserReceiving && !_globalState.UserReceiving.Contains(user.telegramid))
            {
                await SendTextMessageAsync(user, MessageName.AbortedSuccessfully, cancellationToken);
                break;
            }

            var result = await SendFileAsync(part, user.telegramid, cancellationToken);

            if (result == SendFileResult.BLOCKED_BY_USER) //To prevent sending many errors to admin.
                break;

            count++;
        }

        return count;
    }

    private async Task DownloadContentIfNeededAsync(SheetSearchResult file, string folder)
    {
        switch (file.Source)
        {
            case FileSource.Dropbox:
            {
                file.SetLocalPath(Path.Combine(folder, file.Address));

                if (!File.Exists(file.LocalPath))
                {
                    var content = await _dropboxService.DownloadFileAsync(file.ServerPath);
                    await File.WriteAllBytesAsync(file.LocalPath, content);
                }

                break;
            }
            case FileSource.CrawlerDownloadLink:
                await _systemHelper.DownloadFileIfNeededAsync(file, "pdf");
                break;
        }
    }

    private async Task SendWarningManyMessages(User user, int total, CancellationToken cancellationToken)
    {
        if (total >= 10)
        {
            await SendTextMessageAsync(user.telegramid,
                MessageHelper.GetMessage(user.culture, MessageName.WarningManyMessages, total.ToString()),
                cancellationToken, ParseMode.Markdown);

            await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
        }
    }

    private async Task<SendFileResult> SendFileAsync(SheetSearchResult file, string chatId,
        CancellationToken cancellationToken)
    {
        try
        {
            var keyboard = file.Buttons is null ? null : TelegramHelper.GenerateKeyboard(file.Buttons!);

            if (File.Exists(file.LocalPath))
            {
                await using var stream = new FileStream(file.LocalPath, FileMode.Open, FileAccess.Read);

                var fileInfo = new FileInfo(file.LocalPath);

                if (fileInfo.Extension.Equals(".pdf", StringComparison.InvariantCultureIgnoreCase))
                {
                    await _client.SendDocumentAsync(chatId, InputFile.FromStream(stream, fileInfo.Name),
                        cancellationToken: cancellationToken, replyMarkup: keyboard, caption: file.Caption);
                }
                else
                    await SendPhotoAsync(chatId, InputFile.FromStream(stream), cancellationToken, keyboard: keyboard,
                        caption: file.Caption);
            }
            else
            {
                if (file.Address.Contains(".pdf"))
                {
                    await _client.SendDocumentAsync(chatId, InputFile.FromString(file.Address),
                        cancellationToken: cancellationToken, replyMarkup: keyboard, caption: file.Caption);
                }
                else
                    await SendPhotoAsync(chatId, InputFile.FromString(file.Address), cancellationToken,
                        keyboard: keyboard, caption: file.Caption);
            }

            return SendFileResult.SUCCESS;
        }
        catch (Exception exc) when (exc.Message.Contains("bot was blocked by the user",
                                        StringComparison.InvariantCultureIgnoreCase))
        {
            return SendFileResult.BLOCKED_BY_USER;
        }
        catch (Exception exc)
        {
            await SendToAdminAsync($"Caminho da imagem: {file.Address}\n\n*{exc.Message}*\n\n{exc.StackTrace}",
                cancellationToken);
        }

        return SendFileResult.GENERAL_ERROR;
    }
}