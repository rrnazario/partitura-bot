using Microsoft.Extensions.Caching.Memory;
using TelegramPartHook.Domain.SeedWork;
using TelegramPartHook.Application.Commands;
using TelegramPartHook.Application.DTO;
using TelegramPartHook.Domain.Constants;

namespace TelegramPartHook.Application.Factories
{
    public interface IRequestFactory
    {
        IBaseRequest DefineRequest(Search search);
    }

    public class RequestFactory(
        IMemoryCache cache,
        IAdminConfiguration adminConfiguration,
        IEnumerable<IBotRequest> requests,
        ISearchAccessor searchAccessor)
        : IRequestFactory
    {
        public IBaseRequest DefineRequest(Search search)
        {
            if (TryGetFromCache(search.User.telegramid, out var request))
            {
                Console.WriteLine("Command defined: {0}", request.GetType());

                if (request is not IBotRefreshableRequest refRequest)
                    return request;

                refRequest.Rehydrate(search.Term);
                return refRequest;
            }

            var cmd = requests.FirstOrDefault(f => f.Match(search.Term.ToLowerInvariant()));

            if (cmd is not null)
            {
                Console.WriteLine("Command defined: {0}", cmd.GetType());
                return cmd;
            }


            search.SanitizeTerm();
            searchAccessor.SetCurrentSearch(search);
            return new PerformSearchCommand();
        }

        private bool TryGetFromCache(string telegramId, out IBaseRequest request)
            => cache.TryGetValue(telegramId, out request);
    }
}