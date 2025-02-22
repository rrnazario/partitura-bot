using MongoDB.Bson;
using MongoDB.Driver;
using TelegramPartHook.Application.Factories;
using TelegramPartHook.Application.Interfaces.Searches;
using TelegramPartHook.Application.Services;
using TelegramPartHook.Domain.Constants;

namespace TelegramPartHook.Application.Commands.Contribution;

public record AcceptContributionCommand
    : BaseBotStartsWithRequestCommand
{
    public override string Prefix => "/contribution-accept";
}

public class AcceptContributionCommandHandler : BaseAdminBotRequestCommandHandler<AcceptContributionCommand>
{
    private readonly IMongoDatabase _database;
    private readonly IDropboxService _dropboxService;
    private readonly ITelegramSender _sender;

    public AcceptContributionCommandHandler(ISearchAccessor searchAccessor,
        IAdminConfiguration adminConfiguration,
        IMongoDatabase database,
        IDropboxService dropboxService,
        ITelegramSender sender) : base(searchAccessor, adminConfiguration)
    {
        _database = database;
        _sender = sender;

        _dropboxService = dropboxService;
    }

    public override async Task Handle(AcceptContributionCommand request, CancellationToken cancellationToken)
    {
        var imageId = Search.Term.Replace(request.Prefix, string.Empty).Trim();
        var objImageId = new ObjectId(imageId);

        var contributions = _database.GetCollection<Contribution>(Contribution.CollectionName);
        var contribution = await contributions.FindSync(f => f.Id == objImageId, new FindOptions<Contribution>
            {
                Limit = 1
            }, cancellationToken: cancellationToken)
            .FirstOrDefaultAsync(cancellationToken);

        if (contribution is not null)
        {
            await _dropboxService.UploadFileAsync(contribution.Content,
                contribution.GetNameWithExtension(),
                cancellationToken);

            var deleteResult = await contributions.DeleteOneAsync(f => f.Id == objImageId, cancellationToken);
            if (deleteResult.DeletedCount > 0)
            {
                await _sender.SendToAdminAsync($"Imagem '{contribution.Name}' adicionada ao acervo com sucesso.",
                    cancellationToken);
            }
        }
    }
}