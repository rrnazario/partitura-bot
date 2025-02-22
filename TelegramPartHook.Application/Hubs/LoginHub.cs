using Light.GuardClauses;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using TelegramPartHook.Application.Commands;
using TelegramPartHook.Application.Helpers;
using TelegramPartHook.Application.Services;
using TelegramPartHook.Domain.Aggregations.UserAggregation;

namespace TelegramPartHook.Application.Hubs;

public interface ILoginHub
{
    Task SendAsync(string conn);
}

public class LoginHub
    : Hub<ILoginHub>
{
    private readonly IMemoryCache _cache;
    private readonly ITelegramSender _sender;
    private readonly IUserRepository _userRepository;
    private readonly ILoginService _loginService;

    public const string SuccessLogin = "successLogin";
    public const string FailedLogin = "failedLogin";

    public LoginHub(IMemoryCache cache,
                    ITelegramSender sender,
                    ILoginService loginService,
                    IUserRepository userRepository)
    {
        _cache = cache.MustNotBeNull();
        _sender = sender.MustNotBeNull();
        _loginService = loginService.MustNotBeNull();
        _userRepository = userRepository;
    }

    public override Task OnConnectedAsync()
    {
        var portalUsername = GetPortalUsername();

        var dict = _loginService.TryGetLoginInfo(portalUsername);

        if (dict is null)
        {
            return base.OnConnectedAsync();
        }

        dict[portalUsername] = Context.ConnectionId;
        _cache.Set(_loginService.LoginConnections, dict);

        var user = _userRepository.GetByVipNameAsync(portalUsername).GetAwaiter().GetResult();
        if (user is null)
        {
            return base.OnConnectedAsync();
        }

        var keyboard = TelegramHelper.GenerateTrueFalseKeyboard("/login ");

        var lastMessageId = _sender.SendTextMessageAsync(user.telegramid, $"Deseja confirmar o login no portal VIP?\n\nBrowser: {GetBrowserInfo()}", CancellationToken.None, keyboard: keyboard).GetAwaiter().GetResult();

        var command = new LoginHandlerCommand(
            user,
            lastMessageId,
            LoginHandlingState.ConfirmReceived,
            Context.ConnectionId);

        _cache.Set(user.telegramid, command);

        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception exception)
    {
        var dict = _loginService.TryGetLoginInfo(GetPortalUsername());

        if (dict is not null)
        {
            dict.Remove(GetPortalUsername());

            _cache.Set(_loginService.LoginConnections, dict);
        }

        return base.OnDisconnectedAsync(exception);
    }

    private string GetPortalUsername() => GetInfoFromRequest("login");
    private string GetBrowserInfo() => GetInfoFromRequest("browser");
    private string GetInfoFromRequest(string info) => Context.GetHttpContext().Request.Query[info].ToString();
}
