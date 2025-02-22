using System.Globalization;
using Microsoft.EntityFrameworkCore;
using System.Text;
using TelegramPartHook.Application.Commands;
using TelegramPartHook.Application.Factories;
using TelegramPartHook.Application.Services;
using TelegramPartHook.Domain.Aggregations.SearchCacheAggregation;
using TelegramPartHook.Domain.Aggregations.UserAggregation;
using TelegramPartHook.Domain.Constants;
using TelegramPartHook.Infrastructure.Models;
using TelegramPartHook.Infrastructure.Persistence;

namespace TelegramPartHook.Application.Queries
{
    public record GetStatsQuery : BaseBotRequestCommand
    {
        public override string Prefix => "/stats";
    }

    public class GetStatsQueryHandler(
        BotContext context,
        ITelegramSender sender,
        ISearchAccessor searchAccessor,
        IAdminConfiguration adminConfiguration)
        : BaseAdminBotRequestCommandHandler<GetStatsQuery>(searchAccessor, adminConfiguration)
    {
        public override async Task Handle(GetStatsQuery request, CancellationToken cancellationToken)
        {
            var message = new StringBuilder("Status da aplicação:\n\n");
            var users = context.Set<User>().AsNoTracking();

            var totalCache = context.Set<SearchCache>().AsNoTracking().Count();
            message.AppendLine($"*Total de pesquisas no cache: * {totalCache}\n");

            var totalUsers = users.Count();
            message.AppendLine($"*Total de usuários: * {totalUsers}\n");
            
            var totalVipUsers = users
                .Where(u => !string.IsNullOrEmpty(u.vipinformation))
                .ToArray();

            message.AppendLine($"*Total de usuários VIP na história: * {totalVipUsers.Length}\n");

            var countValidVipUsers = totalVipUsers
                .Count(user => user.IsVipValid());

            message.AppendLine($"*Total de usuários VIP válidos: * {countValidVipUsers}\n");

            var totalSearches = users.Select(s => s.searchescount).Sum();
            message.AppendLine($"*Total de pesquisas até hoje: * {totalSearches}\n");

            var totalRepertoiresUsed = users.Count(w => w.Repertoire != null && w.Repertoire.Sheets.Any());
            message.AppendLine($"*Total de usuários que tem repertório: * {totalRepertoiresUsed}\n");

            var lastSearch = users.Select(s => s.lastsearchdate)
                .ToArray()
                .Select(s => DateTime.ParseExact(s, DateConstants.DatabaseFormat, new CultureInfo("pt-BR")))
                .Max();
            message.AppendLine($"*Última pesquisa feita em* {lastSearch:dd/MM/yyyy HH:mm:ss}\n");

            var countErrors = context.Set<AppError>().AsNoTracking().Count();
            message.AppendLine($"*Mensagens de erro: * {countErrors}\n");

            await sender.SendTextMessageAsync(Search.User.telegramid, message.ToString(), cancellationToken);
        }
    }
}