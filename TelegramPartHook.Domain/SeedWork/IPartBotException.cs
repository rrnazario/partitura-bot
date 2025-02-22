using TelegramPartHook.Domain.Aggregations.UserAggregation;
using static TelegramPartHook.Domain.Constants.Enums;

namespace TelegramPartHook.Domain.SeedWork
{
    public interface IPartBotException
    {
    }

    public interface IPartBotCustomException
        : IPartBotException
    {
        MessageName Message { get; }
        bool SendToAdmin { get; }
        User User { get; }
    }

    public interface IPartBotParamException : IPartBotCustomException
    {
        string[] Parameters { get; }
    }

    public abstract class PartBotException(User user) 
        : Exception, IPartBotCustomException
    {
        public new abstract MessageName Message { get; }

        public User User { get; internal set; } = user;

        public virtual bool SendToAdmin => true;
    }

    public abstract class PartBotParamException
        : PartBotException, IPartBotParamException
    {
        public abstract string[] Parameters { get; }

        public PartBotParamException(User user) : base(user)
        {
        }
    }
}