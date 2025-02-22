using TelegramPartHook.Application.Commands;
using TelegramPartHook.Application.Factories;
using TelegramPartHook.Application.Services;
using TelegramPartHook.Domain.Aggregations.UserAggregation;
using TelegramPartHook.Domain.Constants;
using TelegramPartHook.Domain.Exceptions;
using TelegramPartHook.Infrastructure.Helpers;

namespace TelegramPartHook.Application.Queries;

public record GetUserInfoQuery
    : BaseBotStartsWithRequestCommand
{
    public override string Prefix => "/info";
}

public class GetUserInfoQueryHandler(
    IUserRepository userRepository,
    ITelegramSender sender,
    IAdminConfiguration adminConfiguration,
    ISearchAccessor searchAccessor)
    : BaseAdminBotRequestCommandHandler<GetUserInfoQuery>(searchAccessor, adminConfiguration)
{
    // /info userId
    public async override Task Handle(GetUserInfoQuery request, CancellationToken cancellationToken)
    {
        var term = Search.Term.RemoveMultipleSpaces().Trim();
        var userId = GetUserId(request, term);

        var user = await userRepository.GetByIdReadOnlyAsync(userId, cancellationToken);

        if (user is null)
            throw new UserNotFoundException(userId);

        await sender.SendToAdminAsync(
            $"*ID*: {user.telegramid}\n*Full name*: {user.fullname}\n*VIP Info*: {user.vipinformation}",
            cancellationToken);
    }

    private static string GetUserId(GetUserInfoQuery request, string term)
        => term.Replace(request.Prefix, "").Trim();
}