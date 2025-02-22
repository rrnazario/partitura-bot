using VideoLibrary;
using Xabe.FFmpeg;
using TelegramPartHook.Domain.Exceptions;
using TelegramPartHook.Infrastructure.Helpers;
using static TelegramPartHook.Domain.Constants.Enums;
using TelegramPartHook.Application.DTO;
using TelegramPartHook.Application.Factories;
using TelegramPartHook.Application.Helpers;
using TelegramPartHook.Application.Interfaces;

namespace TelegramPartHook.Application.Services
{
    [Obsolete]
    public class UtilitaryService : IUtilitaryService
    {
        private readonly IUserFactory _userFactory;
        private readonly ILogHelper _log;
        private readonly ITelegramSender _sender;
        private readonly ISystemHelper _systemHelper;

        private const int MaxLength = 45000000;

        public UtilitaryService(IUserFactory userFactory, ILogHelper log, ITelegramSender sender, ISystemHelper systemHelper)
        {
            _userFactory = userFactory;
            _log = log;
            _sender = sender;
            _systemHelper = systemHelper;
        }

        public async Task DownloadYoutubeMp3Async(Search search)
        {
            string outputFileName = "", inputFileName = "";

            await _userFactory.PersistUserAsync(search.User);

            await _sender.SendTextMessageAsync(search.User, MessageName.BeginningConversion, CancellationToken.None);

            try
            {
                var youtube = YouTube.Default;
                var youTubeVideo = await youtube.GetVideoAsync(search.Term);
                var youtubeName = string.Join(".", youTubeVideo.FullName.Split(".").SkipLast(1));

                if (youTubeVideo.ContentLength > MaxLength)
                    throw new YoutubeVideoTooLongException(youtubeName);

                inputFileName = youTubeVideo.FullName;
#if DEBUG
                FFmpeg.SetExecutablesPath(@"C:\Users\rrnaz\appdata\Local\ffmpeg");
#endif

                if (!File.Exists(inputFileName))
                    File.WriteAllBytes(youTubeVideo.FullName, youTubeVideo.GetBytes());

                outputFileName = $"{youTubeVideo.FullName.Replace(".mp4", "")}.mp3";

                var result = await FFmpeg.Conversions.FromSnippet.ExtractAudio(youTubeVideo.FullName, outputFileName);
                await result.Start();

                if (File.Exists(outputFileName))
                {
                    using var stream = new FileStream(outputFileName, FileMode.Open, FileAccess.ReadWrite);

                    await _sender.SendTextMessageAsync(search.User, MessageName.VideoDownloaded, CancellationToken.None);

                    //await _sender.SendDocumentAsync(search.User.telegramid, new Telegram.Bot.Types.InputFiles.InputOnlineFile(stream, outputFileName), CancellationToken.None);
                }
            }
            catch (YoutubeVideoTooLongException)
            {
                await _sender.SendTextMessageAsync(search.User, MessageName.TooLongVideo, CancellationToken.None, placeholders: new[] { search.User.fullname });
            }
            catch (Exception e)
            {
                await _sender.SendTextMessageAsync(search.User, MessageName.CannotDownloadVideo, CancellationToken.None, placeholders: new[] { search.User.fullname });

                await _log.ErrorAsync(e, CancellationToken.None);
            }
            finally
            {
                _systemHelper.DeleteFile(outputFileName, inputFileName);
            }
        }
    }
}
