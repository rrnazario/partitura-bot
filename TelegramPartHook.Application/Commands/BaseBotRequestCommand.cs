using MediatR;
using TelegramPartHook.Application.DTO;
using TelegramPartHook.Application.Factories;
using TelegramPartHook.Domain.Constants;
using TelegramPartHook.Domain.Exceptions;
using TelegramPartHook.Domain.SeedWork;

namespace TelegramPartHook.Application.Commands
{
    public abstract record BaseBotRequestCommand : IBotRequest
    {
        public abstract string Prefix { get; }

        public virtual bool Match(string term) => term.Equals(Prefix);
    }

    public abstract record BaseBotStartsWithRequestCommand : BaseBotRequestCommand
    {
        public override bool Match(string term) => term.StartsWith(Prefix, StringComparison.InvariantCultureIgnoreCase);
    }

    public abstract class BaseAdminBotRequestCommandHandler<T>
        : IRequestHandler<T>
        where T: IRequest
    {
        protected readonly Search Search;
        protected BaseAdminBotRequestCommandHandler(
            ISearchAccessor searchAccessor,
            IAdminConfiguration adminConfiguration)
        {
            if (!adminConfiguration.IsUserAdmin(searchAccessor.CurrentSearch().User))
                throw new IgnoreNonAdminException();

            Search = searchAccessor.CurrentSearch();
        }

        public abstract Task Handle(T request, CancellationToken cancellationToken);
    }
}
