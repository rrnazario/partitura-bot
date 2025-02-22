using Light.GuardClauses;
using TelegramPartHook.Domain.Exceptions;
using User = TelegramPartHook.Domain.Aggregations.UserAggregation.User;

namespace TelegramPartHook.Application.DTO
{
    public class Search
    {
        public string Term { get; private set;}
        public User User { get; }

        public Search(string term, User user)
        {
            User = user.MustNotBeNull();
            term.MustNotBeNullOrEmpty((_) => new TooSmallSearchException(user));
            term.Length.MustBeGreaterThan(1, (_, _) => new TooSmallSearchException(user));

            UpdateTerm(term);
        }

        public void UpdateTerm(string newTerm) 
        {
            Term = newTerm.MustNotBeNullOrEmpty(_ => new TooSmallSearchException(User));
        }

        public void SanitizeTerm() => UpdateTerm(Term.Replace("/", ""));
    }
}
