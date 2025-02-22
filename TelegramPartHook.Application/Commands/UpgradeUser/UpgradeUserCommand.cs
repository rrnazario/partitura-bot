using System.Globalization;
using TelegramPartHook.Application.Factories;
using TelegramPartHook.Application.Helpers;
using TelegramPartHook.Application.Services;
using TelegramPartHook.Domain.Aggregations.UserAggregation;
using TelegramPartHook.Domain.Constants;
using TelegramPartHook.Domain.Exceptions;
using TelegramPartHook.Domain.Helpers;
using TelegramPartHook.Domain.SeedWork;
using static TelegramPartHook.Domain.Constants.Enums;

namespace TelegramPartHook.Application.Commands.UpgradeUser
{
    public record UpgradeUserCommand
        : BaseBotStartsWithRequestCommand
    {
        public override string Prefix => "/up";
    };

    public record UpgradeUserInfo
    {
        public string TelegramId { get; }
        public string PortalUser { get; }
        public string ExpireDate { get; }

        public UpgradeUserInfo(string[] info)
        {
            if (info.Length != 3)
            {
                throw new Exception("Invalid upgrade info");
            }

            TelegramId = info.First();
            PortalUser = info.Skip(1).First();
            ExpireDate = info.Skip(2).First();

            if (!DateTime.TryParse(ExpireDate, new CultureInfo("pt-br"), DateTimeStyles.AssumeUniversal, out _))
            {
                throw new Exception($"'{ExpireDate}' is an invalid expired date info");
            }
        }
    }

    public class UpgradeUserCommandHandler(
        IUserRepository repository,
        ITelegramSender sender,
        ISearchAccessor searchAccessor,
        IAdminConfiguration adminConfiguration)
        : BaseAdminBotRequestCommandHandler<UpgradeUserCommand>(searchAccessor, adminConfiguration)
    {
        public override async Task Handle(UpgradeUserCommand request, CancellationToken cancellationToken)
        {
            var info = MountUpgradeUserInfo(request);

            var user = await repository.GetByIdAsync(info.TelegramId, cancellationToken);
            if (user is null)
            {
                throw new UserNotFoundException(info.TelegramId);
            }

            var existsPortalUser = await repository.GetByVipNameAsync(info.PortalUser, cancellationToken);
            if (existsPortalUser is not null && existsPortalUser.telegramid != info.TelegramId)
            {
                await sender.SendToAdminAsync(
                    $"Portal user '{info.PortalUser}' already exists.\nInfo: {existsPortalUser}\n{existsPortalUser.vipinformation}",
                    cancellationToken);
                return;
            }

            user.Upgrade(info.PortalUser, info.ExpireDate);

            repository.Update(user);
            await repository.SaveChangesAsync(cancellationToken);

            var userKey = TelegramHelper.GenerateKeyboard(new KeyboardButtons([
                (MessageHelper.GetMessage(user.culture, MessageName.SeeMyVIPBenefits), "/vip")
            ]));

            await sender.SendTextMessageAsync(user, MessageName.WelcomeToVip, cancellationToken,
                placeholders: [user.fullname], keyboard: userKey);

            await sender.SendToAdminAsync($"User '{user.fullname}' upgraded to VIP.\nInfo: {user.vipinformation}",
                cancellationToken);
        }

        private UpgradeUserInfo MountUpgradeUserInfo(UpgradeUserCommand request)
        {
            var mountedMessage = Search.Term.Replace(request.Prefix, "").Trim();
            var infoArray = mountedMessage.Split(" ").Select(s => s.Trim());

            return new UpgradeUserInfo(infoArray.ToArray());
        }
    }
}