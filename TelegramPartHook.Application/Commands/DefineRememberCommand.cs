using Light.GuardClauses;
using MediatR;
using TelegramPartHook.Domain.Exceptions;
using TelegramPartHook.Domain.SeedWork;
using TelegramPartHook.Infrastructure.Helpers;
using TelegramPartHook.Application.DTO;
using TelegramPartHook.Application.Services;
using TelegramPartHook.Domain.Constants;
using TelegramPartHook.Domain.Aggregations.UserAggregation;
using Serilog;
using TelegramPartHook.Infrastructure.Persistence;
using TelegramPartHook.Application.Factories;
using TelegramPartHook.Domain.Helpers;
using static TelegramPartHook.Domain.Constants.Enums;

namespace TelegramPartHook.Application.Commands
{
    public record DefineRememberCommand
        : BaseBotStartsWithRequestCommand
    {
        public override string Prefix => "/monitorar";
    }

    public class DefineRememberCommandHandler(
        ITelegramSender sender,
        BotContext context,
        IAdminConfiguration adminConfiguration,
        IUnitOfWork unitOfWork,
        ISearchAccessor searchAccessor)
        : IRequestHandler<DefineRememberCommand>
    {
        private readonly Search _search = searchAccessor.CurrentSearch();

        public async Task Handle(DefineRememberCommand request, CancellationToken cancellationToken)
        {
            ValidateCommand(_search);

            var scheduledTerm = ClearTerm(_search.Term);

            try
            {
                _search.User.UpdateScheduledSearch(scheduledTerm);
                context.Update(_search.User);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                await sender.SendTextMessageAsync(_search.User, MessageName.ReminderDefined, cancellationToken,
                    placeholders: [scheduledTerm]);
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
                throw new FailedToDefineRememberException(scheduledTerm, _search.User);
            }
        }

        private void ValidateCommand(Search search)
        {
            ClearTerm(search.Term).MustNotBeNullOrEmpty();

            var remembersCount = search.User.GetMonitoredItems().Length;

            if (remembersCount < 3 || search.User.IsVipValid() || !IsNotAdmin(search.User))
            {
                return;
            }

            var message = MessageHelper.GetMessage(search.User.culture, MessageName.RememberNumberExceeded,
                AdminConstants.AdminLink);

            throw new NotVipUserException(search.User, message);
        }

        private static string ClearTerm(string originalTerm)
        {
            return originalTerm.Replace("/monitorar", "", StringComparison.InvariantCultureIgnoreCase)
                .Trim()
                .RemoveInvalidChars(keepNumbers: true)
                .Replace("%", "");
        }

        private bool IsNotAdmin(User user) => user.telegramid != adminConfiguration.AdminChatId;
    }
}