using MediatR;
using TelegramPartHook.Domain.Aggregations.UserAggregation;
using TelegramPartHook.Domain.SeedWork;

namespace TelegramPartHook.Application.Commands.Repertoire;

public record AddRepertoireCommand(string PortalName, SheetSearchResult Sheet)
    : IRequest;

public class AddRepertoireCommandHandler(IUserRepository userRepository)
    : IRequestHandler<AddRepertoireCommand>
{
    public async Task Handle(AddRepertoireCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByVipNameAsync(request.PortalName, cancellationToken);

        user?.InitializeRepertoire();
        user?.Repertoire?.TryAdd(request.Sheet);

        userRepository.Update(user!);

        await userRepository.SaveChangesAsync(cancellationToken);
    }
}