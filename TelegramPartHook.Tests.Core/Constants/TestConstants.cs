namespace TelegramPartHook.Tests.Core.Constants
{
    public class TestConstants
    {
        public enum EnvironmentType
        {
            Hml,
            Prod
        }

        public static readonly string BaseRequestPath = "Files\\baseRequest.json";
        public static readonly string BaseCallbackRequestPath = "Files\\baseCallbackRequest.json";
        public static readonly string InstagramRequestPath = "Files\\instagramSample.json";
        public static readonly string RequestPlaceholder = "{0}";
        public static readonly string UserIdPlaceholder = "{1}";
    }
}
