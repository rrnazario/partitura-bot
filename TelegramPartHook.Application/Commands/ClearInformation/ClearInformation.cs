using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TelegramPartHook.Application.DTO;
using TelegramPartHook.Application.Services;
using TelegramPartHook.Infrastructure.Helpers;
using TelegramPartHook.Infrastructure.Persistence;

namespace TelegramPartHook.Application.Commands.ClearInformation
{
    public interface IClearInformationFactory
    {
        IClearDatabase Create(Search search);
    }

    public class ClearInformationFactory
        : IClearInformationFactory
    {
        private readonly IServiceScopeFactory _factory;

        public ClearInformationFactory(IServiceScopeFactory serviceScopeFactory)
        {
            _factory = serviceScopeFactory;
        }

        public IClearDatabase Create(Search search)
            => search.Term.Split(' ')[1]
                    .RemoveMultipleSpaces()
                    .Trim() switch
                {
                    var x when x.EndsWith("ok", StringComparison.InvariantCultureIgnoreCase) =>
                        new ClearDatabaseInformation(_factory),
                    var x when x.EndsWith("cache", StringComparison.InvariantCultureIgnoreCase) =>
                        new ClearDatabaseCache(_factory, search),
                    var x when x.EndsWith("error", StringComparison.InvariantCultureIgnoreCase) => new ClearAppError(
                        _factory),
                    _ => throw new NotImplementedException()
                };
    }

    public interface IClearDatabase
    {
        Task ExecuteAsync(CancellationToken cancellationToken);
    }

    public abstract class ClearDatabase
        : IClearDatabase
    {
        private readonly IServiceScopeFactory _factory;

        protected ClearDatabase(IServiceScopeFactory factory)
        {
            _factory = factory;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await using var scope = _factory.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<BotContext>();
            var sender = scope.ServiceProvider.GetRequiredService<ITelegramSender>();

            var totalCleaned = await context.Database.ExecuteSqlRawAsync(Query, cancellationToken);

            await sender.SendToAdminAsync($"{totalCleaned} lines were cleared.", cancellationToken);
        }

        public abstract string Query { get; }
    }

    public class ClearDatabaseInformation
        : ClearDatabase
    {
        public ClearDatabaseInformation(IServiceScopeFactory serviceScopeFactory)
            : base(serviceScopeFactory) { }

        public override string Query => "UPDATE client SET searchesok = NULL";
    }

    public class ClearDatabaseCache
        : ClearDatabase
    {
        public ClearDatabaseCache(IServiceScopeFactory factory, Search search)
            : base(factory)
        {
            var split = search.Term.Split(" ");
            var filter = split.Length > 2
                ? $"LOWER(term) = LOWER('{string.Join(" ", split.Skip(2)).Trim()}')"//clear cache sem perceber
                : "1=1";

            Query = $"DELETE FROM search WHERE {filter}";
        }

        public override string Query { get; }
    }


    public class ClearAppError
        : ClearDatabase
    {
        public ClearAppError(IServiceScopeFactory factory)
        : base(factory) { }

        public override string Query => $"DELETE FROM apperror WHERE 1=1";
    }
}