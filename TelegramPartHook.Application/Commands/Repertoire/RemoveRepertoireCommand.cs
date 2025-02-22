using MediatR;
using TelegramPartHook.Domain.Aggregations.UserAggregation;
using TelegramPartHook.Domain.SeedWork;

namespace TelegramPartHook.Application.Commands.Repertoire;

public record RemoveRepertoireCommand(string PortalName, SheetSearchResult Sheet)
    : IRequest;

public class RemoveRepertoireCommandHandler(IUserRepository repository) 
    : IRequestHandler<RemoveRepertoireCommand>
{
    public async Task Handle(RemoveRepertoireCommand request, CancellationToken cancellationToken)
    {
        var user = await repository.GetByVipNameAsync(request.PortalName, cancellationToken);

        if (user is null)
            return;

        user.Repertoire.Remove(request.Sheet);

        repository.Update(user);
        await repository.SaveChangesAsync(cancellationToken);            
    }
}