using MediatR;
using TelegramPartHook.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using TelegramPartHook.Infrastructure.Models;
using Microsoft.Extensions.DependencyInjection;

namespace TelegramPartHook.Application.Notifications;

public record RemoveDeactivatedUserEvent(string userId) : INotification;

public class RemoveDeactivatedUserNotificationHandler
    : INotificationHandler<RemoveDeactivatedUserEvent>
{
    private readonly IServiceScopeFactory _factory;

    public RemoveDeactivatedUserNotificationHandler(IServiceScopeFactory serviceScopeFactory)
    {
        _factory = serviceScopeFactory;
    }

    public async Task Handle(RemoveDeactivatedUserEvent notification, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(notification.userId)) return;
        
        await using var scope = _factory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<BotContext>();

        try
        {
            await using var t = await context.Database.BeginTransactionAsync(cancellationToken);
            await context.Database.ExecuteSqlRawAsync("DELETE FROM client WHERE telegramid = {0}", notification.userId);
            await t.CommitAsync(cancellationToken);
        }
        catch (Exception e)
        {
            context.Set<AppError>().Add(new AppError(e));
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}