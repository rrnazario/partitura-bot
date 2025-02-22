using System.Reflection;
using TelegramPartHook.Domain.SeedWork;
using static TelegramPartHook.Domain.Constants.Enums;
using TelegramPartHook.Application.Services;
using Microsoft.Extensions.Caching.Memory;
using TelegramPartHook.Domain.Constants;
using User = TelegramPartHook.Domain.Aggregations.UserAggregation.User;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramPartHook.Domain.Helpers;

namespace TelegramPartHook.Application.Commands
{
    public abstract class BotInMemoryCommandHandler<TEnum, TCommand>
    where TEnum : Enum
    where TCommand : class, IBotRefreshableRequest
    {
        protected readonly ITelegramSender Sender;
        protected readonly IAdminConfiguration AdminConfiguration;
        protected readonly IMemoryCache Cache;

        protected BotInMemoryCommandHandler(ITelegramSender sender,
                             IMemoryCache cache,
                             IAdminConfiguration adminConfiguration)
        {
            Sender = sender;
            Cache = cache;
            AdminConfiguration = adminConfiguration;
        }

        public virtual async Task Handle(TCommand request, CancellationToken cancellationToken)
        {
            await Task.Run(() => BotHandle(request), cancellationToken);
        }

        private void BotHandle(TCommand command)
        {
            var enumValue = Enum.GetName(typeof(TEnum), command.GetState());

            var method = GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
            .FirstOrDefault(m => m.Name == enumValue);

            if (method is not null)            
                method.Invoke(this, [command]);            
            else
                throw new NotImplementedException($"You must create a method with exact enum item name '{enumValue}'");
        }

        protected async Task ClearMemoryAsync(User user, int lastMessageId, bool sendFinalizeMessage = true,
            InlineKeyboardMarkup? keyboard = null)
        {
            if (sendFinalizeMessage)
            {
                if (lastMessageId != 0)
                {
                    await Sender.EditMessageTextAsync(user.telegramid, lastMessageId, MessageHelper.GetMessage(user.culture, MessageName.OperationFinalized), CancellationToken.None, keyboard: keyboard);
                }
                else
                    await Sender.SendTextMessageAsync(user, MessageName.OperationFinalized, CancellationToken.None, keyboard: keyboard);
            }
            Cache.Remove(user.telegramid);
        }
    }
}
