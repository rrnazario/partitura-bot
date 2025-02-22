using MediatR;
using TelegramPartHook.Domain.Aggregations.UserAggregation;
using TelegramPartHook.Domain.SeedWork;

namespace TelegramPartHook.Application.Queries.Portal;

public record GetRepertoireQuery(string PortalName)
    : IRequest<RepertoireResponse>;

public record RepertoireResponse
{
    public List<SheetSearchResult> Sheets { get; }

    public RepertoireResponse(User user)
    {
        user.InitializeRepertoire();

        Sheets = user.Repertoire!.Sheets;
    }
}

public class GetRepertoireQueryHandler(IUserRepository repository)
    : IRequestHandler<GetRepertoireQuery, RepertoireResponse?>
{
    public async Task<RepertoireResponse?> Handle(GetRepertoireQuery request, CancellationToken cancellationToken)
    {
        var user = await repository.GetByVipNameAsync(request.PortalName, cancellationToken);

        return user != null
            ? new RepertoireResponse(user)
            : null;
    }
}