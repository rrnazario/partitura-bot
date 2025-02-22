using TelegramPartHook.Application.Factories;
using TelegramPartHook.Domain.Constants;

namespace TelegramPartHook.Application.Commands.ClearInformation
{
    public record ClearInformationCommand
        : BaseBotStartsWithRequestCommand
    { 
        public override string Prefix => "/clear";
    }

    public class ClearInformationCommandHandler(
        IClearInformationFactory factory,
        IAdminConfiguration adminConfiguration,
        ISearchAccessor searchAccessor)
        : BaseAdminBotRequestCommandHandler<ClearInformationCommand>(searchAccessor, adminConfiguration)
    {
        public override async Task Handle(ClearInformationCommand request, CancellationToken cancellationToken)
        {
            var clearAction = factory.Create(Search);

            await clearAction.ExecuteAsync(cancellationToken);
        }
    }
}
