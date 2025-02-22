#nullable enable
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TelegramPartHook.Domain.SeedWork;

namespace TelegramPartHook.Infrastructure.Persistence.Repositories;

public abstract class RepositoryBase<TEntity>(BotContext context) 
    : IRepository<TEntity>
    where TEntity : Entity
{
    public IQueryable<TEntity> GetAll() => context.Set<TEntity>();
    public IQueryable<TEntity> GetAllReadOnly() => GetAll().AsNoTracking();

    public Task<TEntity?> GetSingleReadOnlyAsync(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
        => GetAllReadOnly().FirstOrDefaultAsync(predicate, cancellationToken);

    public Task<TEntity?> GetSingleAsync(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
        => GetAll().FirstOrDefaultAsync(predicate, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => context.SaveChangesAsync(cancellationToken);

    public void Add(TEntity entity) => context.Add(entity);
    public void Update(TEntity entity) => context.Update(entity);
}