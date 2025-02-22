using TelegramPartHook.Domain.SeedWork;

namespace TelegramPartHook.Domain.Exceptions
{
    public class IgnoreNonAdminException
        : Exception, IPartBotException
    {
        public IgnoreNonAdminException() : base() { }        
    }
}
