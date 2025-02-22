using TelegramPartHook.Domain.Aggregations.UserAggregation;
using TelegramPartHook.Domain.SeedWork;
using static TelegramPartHook.Domain.Constants.Enums;

namespace TelegramPartHook.Domain.Exceptions
{
    public class RememberNumberExceededException
        : PartBotException
    {
        public RememberNumberExceededException(User user) : base(user) { }

        public override MessageName Message => MessageName.RememberNumberExceeded;
    }
}
