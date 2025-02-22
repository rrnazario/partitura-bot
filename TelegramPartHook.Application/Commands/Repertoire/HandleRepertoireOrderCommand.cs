using MediatR;
using TelegramPartHook.Domain.Aggregations.UserAggregation;
using TelegramPartHook.Domain.SeedWork;

namespace TelegramPartHook.Application.Commands.Repertoire
{
    public record HandleRepertoireOrderCommand(
        string PortalName,
        SheetSearchResult Sheet,
        RepertoireOrder Order)
         : IRequest
    {
    }

    public enum RepertoireOrder
    {
        Up,
        Down,
        First,
        Last,
        Clear
    }

    public class HandleRepertoireOrderCommandHandler(IUserRepository repository) 
        : IRequestHandler<HandleRepertoireOrderCommand>
    {
        public async Task Handle(HandleRepertoireOrderCommand request, CancellationToken cancellationToken)
        {
            var user = await repository.GetByVipNameAsync(request.PortalName, cancellationToken);

            user?.InitializeRepertoire();

            switch (request.Order)
            {
                case RepertoireOrder.Up:
                    user?.Repertoire?.Up(request.Sheet);
                    break;
                case RepertoireOrder.Down:
                    user?.Repertoire?.Down(request.Sheet);
                    break;
                case RepertoireOrder.First:
                    user?.Repertoire?.First(request.Sheet);
                    break;
                case RepertoireOrder.Last:
                    user?.Repertoire?.Last(request.Sheet);
                    break;
                case RepertoireOrder.Clear:
                    user?.Repertoire?.Clear();
                    break;
            }

            repository.Update(user!);
            await repository.SaveChangesAsync(cancellationToken);            
        }
    }
}
