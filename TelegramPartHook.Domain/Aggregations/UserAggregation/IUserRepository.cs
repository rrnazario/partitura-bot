#nullable enable
using System.Threading;
using TelegramPartHook.Domain.SeedWork;

namespace TelegramPartHook.Domain.Aggregations.UserAggregation;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByVipNameAsync(string portalName, CancellationToken cancellationToken = default);
    Task<User?> GetByIdAsync(string portalName, CancellationToken cancellationToken = default);
    Task<User?> GetByIdReadOnlyAsync(string telegramId, CancellationToken cancellationToken = default);
}