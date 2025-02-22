using Light.GuardClauses;
using Microsoft.Extensions.Caching.Memory;
using TelegramPartHook.Application.Requests;
using TelegramPartHook.Domain.Aggregations.UserAggregation;

namespace TelegramPartHook.Application.Services
{
    public interface ILoginService
    {
        Task<bool> VerifyUserAsync(LoginRequest loginRequest);
        Task<bool> LogoutAsync(LogoutRequest logoutRequest);
        Dictionary<string, string>? TryGetLoginInfo(string login);

        string LoginConnections { get; }
    }

    public class LoginService
        : ILoginService
    {
        private readonly IUserRepository _repository;
        private readonly IMemoryCache _cache;

        public LoginService(IUserRepository repository, IMemoryCache cache)
        {
            _repository = repository;
            _cache = cache;
        }

        public string LoginConnections => nameof(LoginConnections);

        public async Task<bool> LogoutAsync(LogoutRequest logoutRequest)
        {
            logoutRequest.MustNotBeNull();
            logoutRequest.Login.MustNotBeNullOrEmpty();
            logoutRequest.Token.MustNotBeNullOrEmpty();

            var user = await _repository.GetByVipNameAsync(logoutRequest.Login);
            var removed = user!.RemoveToken(logoutRequest.Token);

            _repository.Update(user);
            await _repository.SaveChangesAsync();

            return removed;
        }

        public async Task<bool> VerifyUserAsync(LoginRequest loginRequest)
        {
            loginRequest.MustNotBeNull();
            loginRequest.Login.MustNotBeNullOrEmpty();

            var user = await _repository.GetByVipNameAsync(loginRequest.Login);

            var isValidUser = user is not null && user.IsVipValid();

            if (isValidUser)
                AssignToCache(user);

            return isValidUser;
        }

        public Dictionary<string, string>? TryGetLoginInfo(string login)
            => _cache.TryGetValue<Dictionary<string, string>>(LoginConnections, out var dict) && dict!.ContainsKey(login)
               ? dict
               : null;

        private void AssignToCache(User? user)
        {
            var portalUsername = user!.GetPortalUsername();

            var dict = TryGetLoginInfo(portalUsername) ??
                       new Dictionary<string, string>();

            dict[portalUsername] = string.Empty;
            _cache.Set(LoginConnections, dict);
        }


    }
}
