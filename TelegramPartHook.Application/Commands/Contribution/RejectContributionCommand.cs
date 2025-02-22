using MongoDB.Bson;
using MongoDB.Driver;
using TelegramPartHook.Application.Factories;
using TelegramPartHook.Application.Services;
using TelegramPartHook.Domain.Constants;

namespace TelegramPartHook.Application.Commands.Contribution;

public record RejectContributionCommand
    : BaseBotStartsWithRequestCommand
{
    public override string Prefix => "/contribution-reject";
}

public class RejectContributionCommandHandler : BaseAdminBotRequestCommandHandler<RejectContributionCommand>
{
    private readonly IMongoDatabase _database;
    private readonly ITelegramSender _sender;

    public RejectContributionCommandHandler(ISearchAccessor searchAccessor,
        IAdminConfiguration adminConfiguration,
        IMongoDatabase database,
        ITelegramSender sender) : base(searchAccessor, adminConfiguration)
    {
        _database = database;
        _sender = sender;
    }

    public override async Task Handle(RejectContributionCommand request, CancellationToken cancellationToken)
    {
        var imageId = Search.Term.Replace(request.Prefix, string.Empty).Trim();
        var objImageId = new ObjectId(imageId);

        var contributions = _database.GetCollection<Contribution>(Contribution.CollectionName);
        var deleteResult = await contributions.DeleteOneAsync(f => f.Id == objImageId, cancellationToken);
        if (deleteResult.DeletedCount > 0)
        {
            await _sender.SendToAdminAsync($"Imagem '{imageId}' rejeitada com sucesso.",
                cancellationToken);
            return;
        }
        
        await _sender.SendToAdminAsync($"Imagem '{imageId}' não encontrada no acervo.",
            cancellationToken);
    }
}