using MediatR;
using Telegram.Bot.Types.Enums;
using TelegramPartHook.Application.DTO;
using TelegramPartHook.Application.Factories;
using TelegramPartHook.Application.Helpers;
using TelegramPartHook.Application.Services;
using TelegramPartHook.Domain.Aggregations.UserAggregation;
using TelegramPartHook.Domain.Constants;
using TelegramPartHook.Domain.Exceptions;

namespace TelegramPartHook.Application.Commands
{
    public record SendAdvertisingCommand
        : BaseBotStartsWithRequestCommand
    {
        public override string Prefix => "/ad";
    }

    public class SendAdvertisingCommandHandler
        : IRequestHandler<SendAdvertisingCommand>
    {
        private readonly ITelegramSender _sender;
        private readonly ILogHelper _log;
        private readonly IGlobalState _global;
        private readonly IAdminConfiguration _adminConfiguration;
        private readonly IUserRepository _repository;
        private readonly Search _search;

        public SendAdvertisingCommandHandler(
            ITelegramSender sender,
            ILogHelper log,
            IGlobalState global,
            IUserRepository repository,
            IAdminConfiguration adminConfiguration,
            ISearchAccessor searchAccessor)
        {
            if (!adminConfiguration.IsUserAdmin(searchAccessor.CurrentSearch().User))
                throw new IgnoreNonAdminException();

            _sender = sender;
            _log = log;
            _global = global;
            _repository = repository;
            _adminConfiguration = adminConfiguration;

            _search = searchAccessor.CurrentSearch();

        }

        public async Task Handle(SendAdvertisingCommand request, CancellationToken cancellationToken)
        {
            var users = await GetAllNotifiableAsync();

            var msg = _search.Term.Replace(request.Prefix, "", StringComparison.InvariantCultureIgnoreCase).Trim();

            int success = 0, failure = 0;
            foreach (var user in users)
            {
                try
                {
                    _log.Info($"Enviando ad para usuario '{user.fullname}'...", cancellationToken);

                    var messageId = await _sender.SendTextMessageAsync(user.telegramid, msg, cancellationToken);

                    _log.Info($"Enviado para usuario '{user.fullname}'.", cancellationToken);

                    if (messageId != default) success++; else failure++;
                }
                catch (Exception e)
                {
                    _log.Info(e, cancellationToken);

                    failure++;
                }
            }

            await _sender.SendToAdminAsync(
                $"Ad sent.\n{users.Count()} total\n{success} successful.\n{failure} failures.",
                cancellationToken, ParseMode.Markdown);
        }

        private async Task<List<User>> GetAllNotifiableAsync()
            => _global.IsDebug
                ? [new("Rogim", "Nazario", _adminConfiguration.AdminChatId)]
                : await Task.FromResult(_repository.GetAllReadOnly().Where(user => !user.unsubscribe).ToArray().Where(user => !user.IsVipValid()).ToList());
    }
}