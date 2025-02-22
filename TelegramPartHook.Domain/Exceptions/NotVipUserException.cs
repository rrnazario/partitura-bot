using TelegramPartHook.Domain.Aggregations.UserAggregation;
using TelegramPartHook.Domain.Constants;
using TelegramPartHook.Domain.SeedWork;
using static TelegramPartHook.Domain.Constants.Enums;

namespace TelegramPartHook.Domain.Exceptions
{
    public class NotVipUserException
        : PartBotParamException
    {
        public NotVipUserException(User user) : base(user) { }
        public NotVipUserException(User user, string customMessage) : base(user)
        {
            CustomMessage = customMessage;
        }

        public override string[] Parameters => [AdminConstants.AdminLink];

        public override MessageName Message => MessageName.NotVipUser;

        public string CustomMessage { get; set; } = string.Empty;

        public override bool SendToAdmin => false;
    }
}
