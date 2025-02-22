using MediatR;
using TelegramPartHook.Application.DTO;
using TelegramPartHook.Application.Factories;
using TelegramPartHook.Application.Helpers;
using TelegramPartHook.Application.Services;
using TelegramPartHook.Domain.Aggregations.UserAggregation;
using TelegramPartHook.Domain.Constants;
using TelegramPartHook.Domain.Exceptions;

namespace TelegramPartHook.Application.Commands
{
    public record SendDirectMessageCommand
        : BaseBotStartsWithRequestCommand, IRequest
    {
        public override string Prefix => "/private ";
    }

    public class SendDirectMessageCommandHandler
        : IRequestHandler<SendDirectMessageCommand>
    {
        private readonly IUserRepository _repository;
        private readonly ITelegramSender _sender;
        private readonly Search _search;

        public SendDirectMessageCommandHandler(
            IUserRepository repository,
            ITelegramSender sender,
            IAdminConfiguration adminConfiguration,
            ISearchAccessor searchAccessor)
        {
            if (!adminConfiguration.IsUserAdmin(searchAccessor.CurrentSearch().User))
                throw new IgnoreNonAdminException();

            _repository = repository;
            _sender = sender;
            _search = searchAccessor.CurrentSearch();
        }

        // /private id1,id2,id3;message
        public async Task Handle(SendDirectMessageCommand request, CancellationToken cancellationToken)
        {
            var direct = new DirectMessage(_search.Term.Replace(request.Prefix, ""));

            if (direct.IsValid())
            {
                var message = direct.Message;
                var keyboard = TelegramHelper.TryGenerateKeyboard(ref message);
                direct.Message = message;

                foreach (var targetTelegramId in direct.TelegramIds)
                {
                    var targetUser = await _repository.GetByIdReadOnlyAsync(targetTelegramId.Trim(), cancellationToken);
                    if (targetUser is not null)
                    {
                        var newMessage = MessageParser.PersonalizeMessage(direct.Message, targetUser!);

                        var messageId = await _sender.SendTextMessageAsync(targetUser.telegramid, newMessage, cancellationToken, keyboard: keyboard);

                        await _sender.SendToAdminAsync(
                        messageId == 0
                        ? $"Error to send message to {targetUser!.fullname}."
                        : $"Message '{newMessage}' sent to user {targetUser!.fullname}.", cancellationToken);
                    }
                    else
                    {
                        await _sender.SendToAdminAsync($"User {targetTelegramId} not found.", cancellationToken);
                    }
                }
            }
        }

        private record DirectMessage
        {
            public IReadOnlyCollection<string> TelegramIds { get; private set; }
            public string Message { get; set; }


            public DirectMessage(string message)
            {
                var split = message.Split(";");

                TelegramIds = [..split.First().Trim().Split(",").Distinct()];
                Message = string.Join(";", split.Skip(1)).Trim();
            }

            public bool IsValid() => TelegramIds.Any();
        }
    }


}
