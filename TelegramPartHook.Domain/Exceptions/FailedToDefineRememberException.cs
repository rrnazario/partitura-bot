using TelegramPartHook.Domain.Aggregations.UserAggregation;
using TelegramPartHook.Domain.SeedWork;
using static TelegramPartHook.Domain.Constants.Enums;

namespace TelegramPartHook.Domain.Exceptions
{
    public class FailedToDefineRememberException
        : PartBotParamException
    {
        public override MessageName Message => MessageName.FailedToDefineRemember;

        private string[] _parameters;
        public override string[] Parameters => _parameters;

        public FailedToDefineRememberException(string term, User user) : base(user)
        {
            _parameters = new[] { term };
        }
    }
}
