using MongoDB.Driver;
using Telegram.Bot.Types;
using TelegramPartHook.Application.Commands;
using TelegramPartHook.Application.Commands.Contribution;
using TelegramPartHook.Application.Factories;
using TelegramPartHook.Application.Helpers;
using TelegramPartHook.Application.Services;
using TelegramPartHook.Domain.Constants;
using TelegramPartHook.Domain.SeedWork;

namespace TelegramPartHook.Application.Queries;

public record GetContributionsQuery
    : BaseBotRequestCommand
{
    public override string Prefix => "/contributions";
}

public class GetContributionsQueryHandler(
    ISearchAccessor searchAccessor,
    IAdminConfiguration adminConfiguration,
    ITelegramSender sender,
    IMongoDatabase database,
    IAdminConfiguration config)
    : BaseAdminBotRequestCommandHandler<GetContributionsQuery>(searchAccessor, adminConfiguration)
{
    public override async Task Handle(GetContributionsQuery request, CancellationToken cancellationToken)
    {
        var contributions = database.GetCollection<Contribution>(Contribution.CollectionName);
        var cursor = await contributions.FindAsync(f => !string.IsNullOrEmpty(f.Name),
            new FindOptions<Contribution>
            {
                Limit = 10
            }, cancellationToken);

        var hasContent = false;

        while (await cursor.MoveNextAsync(cancellationToken))
        {
            foreach (var contribution in cursor.Current)
            {
                await SendContentAsync(contribution, cancellationToken);
                hasContent = true;
            }
        }

        if (!hasContent)
            await sender.SendToAdminAsync("Não há contribuições.", cancellationToken);
    }

    private async Task SendContentAsync(Contribution contribution, CancellationToken cancellationToken)
    {
        var buttons = new List<(string caption, string url)>
        {
            ("Aceitar", $"/contribution-accept {contribution.Id}"),
            ("Rejeitar", $"/contribution-reject {contribution.Id}"),
            ("Editar texto", $"/contribution-edit {contribution.Id}"),
        };
        var keyboard = TelegramHelper.GenerateKeyboard(new KeyboardButtons(buttons));

        using var contents = ImageParser.FromBase64ToStream(contribution.Content);

        if (contribution.GetExtension().Equals("pdf", StringComparison.InvariantCultureIgnoreCase))
        {
            var file = InputFile.FromStream(contents, fileName: contribution.GetNameWithExtension());

            await sender.SendFileAsync(config.AdminChatId,
                file,
                keyboard: keyboard,
                cancellationToken: cancellationToken);
        }
        else
        {
            await sender.SendPhotoAsync(config.AdminChatId,
                InputFile.FromStream(contents),
                caption: contribution.Name,
                keyboard: keyboard,
                cancellationToken: cancellationToken);
        }

        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
    }
}