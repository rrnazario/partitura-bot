namespace TelegramPartHook.Domain.Exceptions
{
    [Obsolete("We no longer will download youtube videos")]
    public class YoutubeVideoTooLongException 
        : Exception
    {
        public YoutubeVideoTooLongException(string message) : base(message) { }
    }
}
