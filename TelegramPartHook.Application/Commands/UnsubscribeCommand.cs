using MediatR;
using TelegramPartHook.Application.Factories;
using TelegramPartHook.Application.Services;
using TelegramPartHook.Domain.Aggregations.UserAggregation;
using TelegramPartHook.Infrastructure.Persistence;
using static TelegramPartHook.Domain.Constants.Enums;

namespace TelegramPartHook.Application.Commands
{
    public record UnsubscribeCommand : BaseBotRequestCommand
    {
        public override string Prefix => "/descadastrar";
    }

    public class UnsubscribeCommandHandler : IRequestHandler<UnsubscribeCommand>
    {
        private readonly ITelegramSender _sender;
        private readonly BotContext _context;
        
        private readonly User _user;

        public UnsubscribeCommandHandler(ITelegramSender sender,
            BotContext context,
            ISearchAccessor searchAccessor)
        {
            _sender = sender;
            _context = context;
            _user = searchAccessor.CurrentSearch().User;
        }

        public async Task Handle(UnsubscribeCommand request, CancellationToken cancellationToken)
        {
            _user.Unsubscribe();

            _context.Update(_user);

            await _context.SaveChangesAsync(cancellationToken);

            await _sender.SendTextMessageAsync(_user, MessageName.Unsubscribed, cancellationToken);            
        }
    }
}