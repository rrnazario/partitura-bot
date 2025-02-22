using System.Globalization;
using static TelegramPartHook.Domain.Constants.Enums;

namespace TelegramPartHook.Domain.Helpers
{
    public class MessageHelper
    {
        private static readonly Random Random = new();

        private static string[] SearchingMessages(string culture) =>
        [
            GetMessage(culture, MessageName.SearchingMessage1),
            GetMessage(culture, MessageName.SearchingMessage2),
            GetMessage(culture, MessageName.SearchingMessage3)
        ];

        private static string[] NotFoundMessages(string culture, string fullname) =>
        [
            GetMessage(culture, MessageName.NotFoundMessage1, fullname),
            GetMessage(culture, MessageName.NotFoundMessage2, fullname),
            GetMessage(culture, MessageName.NotFoundMessage3, fullname),
            GetMessage(culture, MessageName.NotFoundMessage4, fullname)
        ];

        private static (string, string, string)[] SuccessMessages(string culture)
        {
            var instagramButtonInfo = (buttonCaption: GetMessage(culture, MessageName.FollowUsInstagram), buttonUrl: @"https://instagram.com/partiturabot");

            return
            [
                (GetMessage(culture, MessageName.SuccessMessage1), instagramButtonInfo.buttonCaption, instagramButtonInfo.buttonUrl),
                (GetMessage(culture, MessageName.SuccessMessage2), instagramButtonInfo.buttonCaption, instagramButtonInfo.buttonUrl),
                (GetMessage(culture, MessageName.SuccessMessage3), instagramButtonInfo.buttonCaption, instagramButtonInfo.buttonUrl),
                (GetMessage(culture, MessageName.SuccessMessage4), instagramButtonInfo.buttonCaption, instagramButtonInfo.buttonUrl),
                (GetMessage(culture, MessageName.SuccessMessage5), instagramButtonInfo.buttonCaption, instagramButtonInfo.buttonUrl)
            ];
        }

        public static string GetRandomNotFoundMessage(string fullname, string culture)
        {
            var msgs = NotFoundMessages(culture, fullname);
            return msgs[Random.Next(0, msgs.Length)];
        }
        public static (string text, string buttonCaption, string buttonUrl) GetRandomSuccessMessage(string culture)
        {
            var msgs = SuccessMessages(culture);
            return msgs[Random.Next(0, msgs.Length)];
        }
        public static string GetRandomSearchingMessage(string culture)
        {
            var msgs = SearchingMessages(culture);
            return msgs[Random.Next(0, msgs.Length)];
        }

        public static string GetMessage(string culture, MessageName messageName, params string[] placeholders)
        {
            var msg = Domain.Resources.Messages.ResourceManager.GetString(messageName.ToString(), new CultureInfo(culture ?? "en"));

            return GetMessage(msg, placeholders);
        }

        public static string GetMessage(string message, params string[] placeholders)
        {
            if (placeholders.Any())
                message = string.Format(message, placeholders);

            return message.Replace(@"\n", "\n");
        }
    }
}
