using System.Threading;

namespace TelegramPartHook.Domain.SeedWork;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<int> ExecuteSqlRawAsync(string sql, CancellationToken cancellationToken = default);
}