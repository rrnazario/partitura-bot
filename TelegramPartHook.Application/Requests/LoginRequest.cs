namespace TelegramPartHook.Application.Requests
{
    public class LoginRequest
    {
        public string Login { get; set; }
    }

    public class LogoutRequest
    {
        public string Login { get; set; }
        public string Token { get; set; }
    }
}
