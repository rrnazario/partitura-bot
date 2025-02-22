using MediatR;
using TelegramPartHook.Infrastructure.Persistence;

namespace TelegramPartHook.Application.Queries.Portal
{
    public record GetLastSearchesQuery
        : IRequest<LastSearchesResponse>;

    public record LastSearchesResponse(List<string> LastSearches);

    public class GetLastSearchesQueryHandler
        : IRequestHandler<GetLastSearchesQuery, LastSearchesResponse>
    {
        private readonly BotContext _context;

        public GetLastSearchesQueryHandler(BotContext context)
        {
            _context = context;
        }

        public Task<LastSearchesResponse> Handle(GetLastSearchesQuery request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
