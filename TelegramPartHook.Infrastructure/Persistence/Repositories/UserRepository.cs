#nullable enable
using System.Threading;
using System.Threading.Tasks;
using TelegramPartHook.Domain.Aggregations.UserAggregation;

namespace TelegramPartHook.Infrastructure.Persistence.Repositories;

public class UserRepository(BotContext context) 
    : RepositoryBase<User>(context), IUserRepository
{
    public Task<User?> GetByVipNameAsync(string portalName, CancellationToken cancellationToken = default)
        => GetSingleAsync(user => user.isvip && user.vipinformation.EndsWith($"\n{portalName}"), cancellationToken);
    
    public Task<User?> GetByIdAsync(string telegramId, CancellationToken cancellationToken = default)
        => GetSingleAsync(user => user.telegramid == telegramId, cancellationToken);
    
    public Task<User?> GetByIdReadOnlyAsync(string telegramId, CancellationToken cancellationToken = default)
        => GetSingleAsync(user => user.telegramid == telegramId, cancellationToken);
}