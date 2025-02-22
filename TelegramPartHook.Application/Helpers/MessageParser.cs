using TelegramPartHook.Domain.Aggregations.UserAggregation;

namespace TelegramPartHook.Application.Helpers;

public class MessageParser
{
    public static string PersonalizeMessage(string originalMsg, User user)
        => originalMsg
            .Replace("{user}", !string.IsNullOrEmpty(user.fullname) ? user.fullname : "usuário")
            .Trim();
}