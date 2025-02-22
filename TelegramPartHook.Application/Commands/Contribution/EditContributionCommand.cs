using MediatR;
using Microsoft.Extensions.Caching.Memory;
using MongoDB.Bson;
using MongoDB.Driver;
using TelegramPartHook.Application.DTO;
using TelegramPartHook.Application.Factories;
using TelegramPartHook.Application.Helpers;
using TelegramPartHook.Application.Interfaces.Searches;
using TelegramPartHook.Application.Services;
using TelegramPartHook.Domain.Constants;
using TelegramPartHook.Domain.SeedWork;

namespace TelegramPartHook.Application.Commands.Contribution;

public enum EditContributionState
{
    Init,
    CaptionChanged
}

public record EditContributionCommand
    : BaseBotRefreshableRequest<EditContributionState>
        , IBotRequest
{
    public ObjectId? ImageId { get; set; }

    public EditContributionCommand() : base(EditContributionState.Init)
    {
    }

    public EditContributionCommand(EditContributionState state) : base(state)
    {
    }

    public string Prefix => "/contribution-edit";
    public bool Match(string term) => term.StartsWith(Prefix, StringComparison.InvariantCultureIgnoreCase);
}

public class EditContributionCommandHandler
    : BotInMemoryCommandHandler<EditContributionState, EditContributionCommand>,
        IRequestHandler<EditContributionCommand>
{
    private readonly Search _search;
    private readonly IMongoDatabase _database;
    private readonly IDropboxService _dropboxService;

    public EditContributionCommandHandler(
        ITelegramSender sender,
        IMemoryCache cache,
        IAdminConfiguration adminConfiguration,
        ISearchAccessor searchAccessor,
        IMongoDatabase database,
        IDropboxService dropboxService)
        : base(sender, cache, adminConfiguration)
    {
        _database = database;
        _dropboxService = dropboxService;
        _search = searchAccessor.CurrentSearch();
    }

    private async Task<Unit> Init(EditContributionCommand command)
    {
        if (!AdminConfiguration.IsUserAdmin(_search.User))
        {
            await ClearMemoryAsync(_search.User, command.LastMessageId, sendFinalizeMessage: false);
            return Unit.Value;
        }

        var imageId = _search.Term.Replace(command.Prefix, string.Empty).Trim();
        var objImageId = new ObjectId(imageId);

        command.ImageId = objImageId;
        command.SetNextState(EditContributionState.CaptionChanged);

        var keyboard = new KeyboardButtons([("cancelar", "/cancelar")]);

        await Sender.SendToAdminAsync($"Envie o novo nome para a contribuição '{imageId}':",
            keyboard: TelegramHelper.GenerateKeyboard(keyboard),
            cancellationToken: CancellationToken.None);

        Cache.Set(_search.User.telegramid, command);
        return Unit.Value;
    }

    private async Task<Unit> CaptionChanged(EditContributionCommand command)
    {
        if (command.Term == "/cancelar")
        {
            await ClearMemoryAsync(_search.User, command.LastMessageId);
            return Unit.Value;
        }

        var contributions = _database.GetCollection<Contribution>(Contribution.CollectionName);
        var contribution = await contributions.FindSync(f => f.Id == command.ImageId, new FindOptions<Contribution>
            {
                Limit = 1
            })
            .FirstOrDefaultAsync();

        if (contribution is not null)
        {
            await _dropboxService.UploadFileAsync(contribution.Content,
                $"{command.Term}.{contribution.GetExtension()}",
                CancellationToken.None);

            var deleteResult = await contributions.DeleteOneAsync(f => f.Id == command.ImageId);
            if (deleteResult.DeletedCount > 0)
            {
                await Sender.SendToAdminAsync($"Imagem '{command.Term}' (anteriormente '{contribution.Name}') adicionada ao acervo com sucesso.",
                    CancellationToken.None);
            }

            await ClearMemoryAsync(_search.User, command.LastMessageId, false);
            return Unit.Value;
        }

        await Sender.SendToAdminAsync($"Imagem '{command.ImageId}' não encontrada.", CancellationToken.None);
        await ClearMemoryAsync(_search.User, command.LastMessageId);
        return Unit.Value;
    }
}