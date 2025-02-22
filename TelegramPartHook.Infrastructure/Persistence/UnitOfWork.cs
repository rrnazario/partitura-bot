using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TelegramPartHook.Domain.SeedWork;

namespace TelegramPartHook.Infrastructure.Persistence;

public class UnitOfWork
    : IUnitOfWork
{
    private readonly BotContext _botContext;
    private readonly IMediator _mediator;

    public UnitOfWork(BotContext botContext, IMediator mediator)
    {
        _botContext = botContext;
        _mediator = mediator;
    }

    public async Task<int> ExecuteSqlRawAsync(string sql, CancellationToken cancellationToken = default)
        => await _botContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var allEvents = GetDomainEvents();

        var result = await _botContext.SaveChangesAsync(cancellationToken);

        await DispatchDomainEvents(allEvents);

        return result;
    }

    private IDomainEvent[] GetDomainEvents()
      => _botContext
            .ChangeTracker
            .Entries<Entity>()
            .Select(entity => entity.Entity)
            .SelectMany(agg =>
            {
                var domainEvents = agg.GetDomainEvents();

                agg.ClearEvents();

                return domainEvents;
            }).ToArray();

    private async Task DispatchDomainEvents(IDomainEvent[] allEvents)
    {
        if (allEvents.Any())
        {
            foreach (var domainEvent in allEvents)
            {
                await _mediator.Publish(domainEvent);
            }
        }
    }
}
