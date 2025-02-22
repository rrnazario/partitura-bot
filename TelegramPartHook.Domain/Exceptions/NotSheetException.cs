using TelegramPartHook.Domain.Aggregations.UserAggregation;
using TelegramPartHook.Domain.Constants;
using TelegramPartHook.Domain.SeedWork;

namespace TelegramPartHook.Domain.Exceptions
{
    public class NotSheetException
       : PartBotException
    {
        public NotSheetException(User user) : base(user) { }

        public override Enums.MessageName Message => Enums.MessageName.WeAreUnderMaintenance;
    }
}
