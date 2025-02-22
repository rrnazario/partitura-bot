using Light.GuardClauses;
using MediatR;
using TelegramPartHook.Application.Services;
using TelegramPartHook.Domain.Aggregations.UserAggregation;

namespace TelegramPartHook.Application.Commands.Repertoire;

public record GeneratePDFRepertoireCommand 
    : IRequest<string>
{
    public readonly string PortalName;

    public GeneratePDFRepertoireCommand(string portalName)
    {
        PortalName = portalName.MustNotBeNull();
    }
}

public class GeneratePDFRepertoireCommandHandler(
    IUserRepository userRepository,
    IPdfService pdfService) : IRequestHandler<GeneratePDFRepertoireCommand, string>
{
    public async Task<string> Handle(GeneratePDFRepertoireCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByVipNameAsync(request.PortalName, cancellationToken);

        user?.InitializeRepertoire();

        //Generate PDF
        var pdfPath = await pdfService.GenerateAsync([.. user!.Repertoire.Sheets], "Repertoire");

        return pdfPath;
    }
}