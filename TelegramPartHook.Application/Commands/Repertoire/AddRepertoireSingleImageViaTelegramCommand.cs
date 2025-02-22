using MediatR;
using Telegram.Bot.Types.Enums;
using TelegramPartHook.Application.DTO;
using TelegramPartHook.Application.Factories;
using TelegramPartHook.Application.Services;
using TelegramPartHook.Domain.Aggregations.SearchCacheAggregation;
using TelegramPartHook.Domain.Aggregations.UserAggregation;
using TelegramPartHook.Domain.Constants;
using TelegramPartHook.Domain.Exceptions;
using TelegramPartHook.Domain.SeedWork;

namespace TelegramPartHook.Application.Commands.Repertoire;

public record AddRepertoireSingleImageViaTelegramCommand
    : BaseBotStartsWithRequestCommand
{
    public override string Prefix => RepertoireHelper.AddRepertoireByImageViaTelegramCommandPrefix;
}

public class AddRepertoireSingleImageViaTelegramCommandHandler(
    ISearchCacheRepository searchCacheRepository,
    IUserRepository userRepository,
    ITelegramSender sender,
    ISearchAccessor searchAccessor,
    IAdminConfiguration adminConfiguration)
    : IRequestHandler<AddRepertoireSingleImageViaTelegramCommand>

{
    // /prefix term1 term2 term3 index
    private readonly Search _search = searchAccessor.CurrentSearch();

    public async Task Handle(AddRepertoireSingleImageViaTelegramCommand command, CancellationToken cancellationToken)
    {
        string message;
        if (!_search.User.IsVipValid() &&
            _search.User.Repertoire?.Sheets is not null &&
            _search.User.Repertoire.Sheets.Count >= adminConfiguration.MaxFreeSheetsOnRepertoire)
        {
            message =
                "Limite para usuários comuns alcançado. Considere se tornar um membro VIP para adicionar partituras infinitas.\n\nFaça o PIX acima e em seguida envie uma mensagem para o {0}\n\nPara ver seu repertório, envie /repertorio";

            throw new NotVipUserException(_search.User, message);
        }

        var (clearedTerm, index) = GetInfoFromCommand(command);

        if (!TryGetImageFromCache(clearedTerm, index, out var imageToAdd))
        {
            await sender.SendTextMessageAsync(_search.User.telegramid, "Houve um erro ao processar o repertório.",
                cancellationToken, ParseMode.Markdown);
            return;
        }

        _search.User.InitializeRepertoire();
        var added = _search.User.Repertoire!.TryAdd(imageToAdd);

        if (added)
        {
            userRepository.Update(_search.User);
            await userRepository.SaveChangesAsync(cancellationToken);
            message = "Partitura adicionada com sucesso. Para ver seu repertório, envie /repertorio";
        }
        else
        {
            message = "Partitura já havia sido adicionada anteriormente. Para ver seu repertório, envie /repertorio";
        }

        await sender.SendTextMessageAsync(_search.User.telegramid, message, cancellationToken, ParseMode.Markdown);
    }

    private bool TryGetImageFromCache(string term, int index, out SheetSearchResult? result)
    {
        result = null;
        var cache = searchCacheRepository.GetByTermAsync(term).GetAwaiter().GetResult();

        if (cache is null)
        {
            return false;
        }

        result = cache.Results[index];
        return true;
    }

    private (string term, int index) GetInfoFromCommand(AddRepertoireSingleImageViaTelegramCommand command)
    {
        // /add term index
        // term index
        var clearedTerm = _search.Term.Replace(command.Prefix, string.Empty).Trim().Split(' ');

        var term = string.Join(" ", clearedTerm.Take(clearedTerm.Length - 1));

        return (term, int.Parse(clearedTerm.Last()));
    }
}