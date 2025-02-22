using MediatR;
using TelegramPartHook.Infrastructure.Persistence;
using TelegramPartHook.Infrastructure.Models;

namespace TelegramPartHook.Application.Notifications;

public record SaveErrorEvent(Exception Exception) : INotification;

public class SaveErrorNotificationHandler(BotContext context) 
    : INotificationHandler<SaveErrorEvent>
{
    public async Task Handle(SaveErrorEvent notification, CancellationToken cancellationToken)
    {
        var appErrors = context.Set<AppError>();

        try
        {
            appErrors.Add(new AppError(notification.Exception));
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            appErrors.Add(new AppError(e));
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}