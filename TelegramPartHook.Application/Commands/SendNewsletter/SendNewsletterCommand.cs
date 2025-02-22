using MediatR;
using Telegram.Bot.Types.Enums;
using TelegramPartHook.Application.DTO;
using TelegramPartHook.Application.Factories;
using TelegramPartHook.Application.Helpers;
using TelegramPartHook.Application.Services;
using TelegramPartHook.Domain.Aggregations.UserAggregation;
using TelegramPartHook.Domain.Constants;
using TelegramPartHook.Domain.Exceptions;

namespace TelegramPartHook.Application.Commands.SendNewsletter
{
    public record SendNewsletterCommand
        : BaseBotStartsWithRequestCommand
    {
        public override string Prefix => "/newsletter";
    }

    public class SendNewsletterCommandHandler
        : IRequestHandler<SendNewsletterCommand>
    {
        private readonly ITelegramSender _sender;
        private readonly ILogHelper _log;
        private readonly IUserRepository _repository;
        private readonly IAdminConfiguration _adminConfiguration;
        private readonly Search _search;


        public SendNewsletterCommandHandler(
            ITelegramSender sender,
            ILogHelper log,
            IUserRepository repository,
            IAdminConfiguration adminConfiguration,
            ISearchAccessor searchAccessor)
        {
            if (!adminConfiguration.IsUserAdmin(searchAccessor.CurrentSearch().User))
                throw new IgnoreNonAdminException();

            _sender = sender;
            _log = log;
            _repository = repository;
            _adminConfiguration = adminConfiguration;
            _search = searchAccessor.CurrentSearch();
        }

        public async Task Handle(SendNewsletterCommand request, CancellationToken cancellationToken)
        {
            var sendNewsletter = SendNewsletterFactory.Create(_search.Term, _repository, _adminConfiguration);

            var users = await sendNewsletter.GetAllNotifiableAsync(_search.Term);

            var msg = sendNewsletter.ReplaceWildcard();

            int success = 0, failure = 0;
            var keyboard = TelegramHelper.TryGenerateKeyboard(ref msg);

            foreach (var user in users)
            {
                try
                {
                    _log.Info($"::: Enviando newsletter para usuario '{user.fullname}'...", cancellationToken);

                    var personalMessage = MessageParser.PersonalizeMessage(msg, user);

                    var result = await _sender.SendTextMessageAsync(user.telegramid, personalMessage, cancellationToken,
                    parseMode: ParseMode.Markdown,
                    keyboard: keyboard);

                    _log.Info($"Enviada para usuario '{user.fullname}'.", cancellationToken);

                    if (result != 0)
                        success++;
                    else
                        failure++;

                    await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken);
                }
                catch (Exception e)
                {
                    await _log.ErrorAsync(e, cancellationToken);

                    failure++;
                }
            }

            await _sender.SendToAdminAsync(
                $"Newsletter sent.\n{users.Count} total\n{success} successful.\n{failure} failures.",
                cancellationToken, ParseMode.Markdown);            
        }
    }
}