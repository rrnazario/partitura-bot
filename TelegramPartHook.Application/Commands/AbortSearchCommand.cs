using MediatR;
using TelegramPartHook.Application.DTO;
using TelegramPartHook.Application.Factories;
using TelegramPartHook.Application.Helpers;

namespace TelegramPartHook.Application.Commands
{
    public record AbortSearchCommand : BaseBotRequestCommand
    {
        public override string Prefix => "/abortar";
    }

    public class AbortSearchCommandHandler
        : IRequestHandler<AbortSearchCommand>
    {
        private readonly IGlobalState _globalState;
        private readonly Search _search;

        public AbortSearchCommandHandler(
            IGlobalState globalState,
            ISearchAccessor searchAccessor)
        {
            _globalState = globalState;

            _search = searchAccessor.CurrentSearch();
        }

        public async Task Handle(AbortSearchCommand request, CancellationToken cancellationToken)
        {            
            await Task.Run(() => _globalState.MarkGlobalStopSending(_search.User.telegramid), cancellationToken);
        }
    }
}
