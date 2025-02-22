using Newtonsoft.Json;
using TelegramPartHook.Application.DTO;

namespace TelegramPartHook.Application.Factories
{
    public interface ISearchAccessor
    {
        Search CurrentSearch();
        void SetCurrentSearch(Search search);
    }

    public interface ISearchFactory
    {
        Task<Search> CreateSearchAsnc(string content);
        Search GetCurrentSearch();
    }

    public class SearchAccessor : ISearchAccessor
    {
        private Search? _search;
        public Search CurrentSearch() => _search ?? throw new ArgumentException("Search not set");
        public void SetCurrentSearch(Search search) => _search = search;
    }

    public class SearchFactory(
        IUserFactory userFactory,
        ISearchAccessor searchAccessor)
        : ISearchFactory
    {
        private Search _current;

        public async Task<Search> CreateSearchAsnc(string content)
        {
            dynamic dynamicContent = JsonConvert.DeserializeObject(content);

            var user = await userFactory.CreateUserDynamicallyAsync(dynamicContent);
            string? term;
            try
            {
                term = (string)dynamicContent["originalDetectIntentRequest"]["payload"]["data"]["text"];

                if (string.IsNullOrEmpty(term))
                    throw new ArgumentNullException(nameof(term));
            }
            catch
            {
                term = (string)dynamicContent["originalDetectIntentRequest"]["payload"]["data"]["callback_query"]["data"];
            }

            _current = new(term, user);

            searchAccessor.SetCurrentSearch(_current);

            return _current;
        }

        public Search GetCurrentSearch() => _current;
    }
}
