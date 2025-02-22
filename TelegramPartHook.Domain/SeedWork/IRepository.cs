#nullable enable
using System.Linq.Expressions;
using System.Threading;

namespace TelegramPartHook.Domain.SeedWork;

public interface IRepository<TEntity>
    where TEntity : Entity
{
    IQueryable<TEntity> GetAllReadOnly();
    IQueryable<TEntity> GetAll();
    Task<TEntity?> GetSingleReadOnlyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<TEntity?> GetSingleAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    void Add(TEntity entity);
    void Update(TEntity entity);
}