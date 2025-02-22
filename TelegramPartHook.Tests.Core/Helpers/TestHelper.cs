using TelegramPartHook.Domain.Aggregations.UserAggregation;
using TelegramPartHook.Infrastructure.Persistence;
using TelegramPartHook.Tests.Core.Constants;

namespace TelegramPartHook.Tests.Core.Helpers
{
    public class TestHelper
    {
        public const string AdminId = "01234567";

        public static readonly User AdminUser =
            new User(AdminId, "admin name")
                .Upgrade("admin", DateTime.Now.AddYears(1).ToString("dd/MM/yyyy"));

        public static string GenerateAdminCommandText(string term)
            => GenerateCommandText(term, AdminId);

        public static string GenerateCommandText(string term, string userId = "")
        {
            userId = GenerateUserIdIfNeeded(userId);

            return File.ReadAllText(TestConstants.BaseCallbackRequestPath)
                .Replace(TestConstants.RequestPlaceholder, term)
                .Replace(TestConstants.UserIdPlaceholder, userId);
        }

        public static string GenerateUserId() => DateTime.Now.ToString("ffffff");

        private static string GenerateUserIdIfNeeded(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                userId = GenerateUserId();
            }

            return userId;
        }
        
        public static User GenerateUser(BotContext context)
        {
            var userTelegramId = GenerateUserId();
            var user = new User(userTelegramId, "User Telegram", "en");
            context.Add(user);
            context.SaveChangesAsync().GetAwaiter().GetResult();

            return user;
        }
    }
}