using MediatR;
using System.Text.RegularExpressions;
using TelegramPartHook.Application.Interfaces.Searches;

namespace TelegramPartHook.Application.Queries.Portal
{
    public record GetAllCollectionListQuery
        : IRequest<IEnumerable<DropboxPortalResponse>>;

    public record DropboxPortalResponse(
        string Address, 
        string ServerPath, 
        string Extension) { }

    public partial class GetAllCollectionListQueryHandler
        : IRequestHandler<GetAllCollectionListQuery, IEnumerable<DropboxPortalResponse>>
    {
        
        [GeneratedRegex(@"^[\d\s\-]{2,}")]
        private static partial Regex NumbersOfBeginingRemoveRegex();

        private readonly IDropboxService _dropboxSearchService;

        public GetAllCollectionListQueryHandler(IDropboxService dropboxSearchService)
        {
            _dropboxSearchService = dropboxSearchService;
        }

        public async Task<IEnumerable<DropboxPortalResponse>> Handle(GetAllCollectionListQuery request, CancellationToken cancellationToken)
        {
            var result = await _dropboxSearchService.GetAllAsync(cancellationToken);

            var numbersOfBeginingRemoveRegex = NumbersOfBeginingRemoveRegex();

            var parsedResult = result.Select(s =>
            {
                var address = string.Join(".", s.Address.Split(".").SkipLast(1));
                address = numbersOfBeginingRemoveRegex.Replace(address, "");

                var extension = s.Address.Split(".").Last();

                return new DropboxPortalResponse(address, s.ServerPath, extension);
            });

            return parsedResult;
        }
    }
}
