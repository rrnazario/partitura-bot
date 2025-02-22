using MediatR;
using Microsoft.Extensions.Caching.Memory;
using TelegramPartHook.Application.Services;
using Microsoft.AspNetCore.SignalR;
using TelegramPartHook.Application.Hubs;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TelegramPartHook.Domain.Constants;
using System.IdentityModel.Tokens.Jwt;
using TelegramPartHook.Domain.Aggregations.UserAggregation;
using TelegramPartHook.Infrastructure.Persistence;
using Serilog;
using Microsoft.Extensions.DependencyInjection;

namespace TelegramPartHook.Application.Commands;

public enum LoginHandlingState
{
    Init,
    ConfirmReceived
}

public record LoginHandlerCommand
    : BaseBotRefreshableRequest<LoginHandlingState>, IRequest
{
    public User User { get; private set; }
    public string? FrontConnection { get; }

    public LoginHandlerCommand(User user, int lastMessageId, LoginHandlingState state, string frontConnection)
        : base(LoginHandlingState.Init)
    {
        User = user;
        LastMessageId = lastMessageId;
        FrontConnection = frontConnection;

        SetNextState(state);
    }
}

public class LoginHandlerCommandHandler
    : BotInMemoryCommandHandler<LoginHandlingState, LoginHandlerCommand>, IRequestHandler<LoginHandlerCommand>
{
    private const string PlaceHolder = "/login ";

    private readonly IHubContext<LoginHub> _hubContext;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public LoginHandlerCommandHandler(ITelegramSender sender,
        IMemoryCache cache,
        IHubContext<LoginHub> hubContext,
        IServiceScopeFactory serviceScopeFactory,
        IAdminConfiguration adminConfiguration)
        : base(sender, cache, adminConfiguration)
    {
        _hubContext = hubContext;
        _serviceScopeFactory = serviceScopeFactory;
    }

    private async Task<Unit> ConfirmReceived(LoginHandlerCommand command)
    {
        await Sender.EditMessageTextAsync(command.User.telegramid, command.LastMessageId,
            "Aguarde...", CancellationToken.None);

        var hubClient = _hubContext.Clients.Client(command.FrontConnection);

        string message;
        if (bool.TryParse(command.Term.Replace(PlaceHolder, ""), out var confirmed) && confirmed)
        {
            message =
                "Login realizado com sucesso!"; //MessageHelper.GetMessage(command.Search.User.culture, MessageName.RememberSuccessfullyRemoved, command.MonitoredItem.Format());

            var token = GenerateToken(command.User.GetPortalUsername());

            await hubClient.SendAsync(LoginHub.SuccessLogin, new { token });

            await HandleUserTokensAsync(command.User, token);
        }
        else
        {
            message = "Login cancelado.";

            await hubClient.SendAsync(LoginHub.FailedLogin);
        }

        await Sender.EditMessageTextAsync(command.User.telegramid, command.LastMessageId,
            message, CancellationToken.None);

        await ClearMemoryAsync(command.User, command.LastMessageId, !confirmed);

        return Unit.Value;
    }

    private async Task HandleUserTokensAsync(User user, string newToken)
    {
        user.UpdateActiveTokens(newToken);

        using var scope = _serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BotContext>();

        context.Update(user);

        try
        {
            var count = await context.SaveChangesAsync();

            Log.Information($"[END] Handling user token. Updated: {count}");
        }
        catch (Exception e)
        {
            Log.Error(e.Message);
            Log.Error(e.InnerException?.Message!);

            throw;
        }
    }

    private string GenerateToken(string portalUserName)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AdminConfiguration.ISK));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.Role, "1"), //vip
        };

        var token = new JwtSecurityToken(AdminConfiguration.Issuer, portalUserName, claims,
            expires: DateTime.UtcNow.AddDays(7), signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}