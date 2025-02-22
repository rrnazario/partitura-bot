using TelegramPartHook.Domain.Aggregations.UserAggregation;
using TelegramPartHook.Domain.SeedWork;
using static TelegramPartHook.Domain.Constants.Enums;

namespace TelegramPartHook.Domain.Exceptions
{
    public class TooSmallSearchException
        : PartBotException
    {
        public TooSmallSearchException(User user) : base(user) { }

        public override MessageName Message => MessageName.TooSmallTerm;

        public override bool SendToAdmin => false;
    }
}
