using TelegramPartHook.Application.Factories;
using TelegramPartHook.Application.Helpers;
using TelegramPartHook.Domain.Constants;

namespace TelegramPartHook.Application.Commands
{
    public record RefreshCacheCommand
        : BaseBotRequestCommand
    {
        public override string Prefix => "/refresh";
    }

    public class RefreshCacheCommandHandler
        : BaseAdminBotRequestCommandHandler<RefreshCacheCommand>
    {
        private readonly IGlobalState _globalState;

        public RefreshCacheCommandHandler(
            IGlobalState globalState,
            ISearchAccessor searchAccessor,
            IAdminConfiguration adminConfiguration)
            : base(searchAccessor, adminConfiguration)
        {
            _globalState = globalState;
        }

        public async override Task Handle(RefreshCacheCommand request, CancellationToken cancellationToken)
        {
            await Task.Run(_globalState.RefreshConfig, cancellationToken);
        }
    }
}
