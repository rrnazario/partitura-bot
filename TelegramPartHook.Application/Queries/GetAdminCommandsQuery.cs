using System.Text;
using TelegramPartHook.Application.Commands;
using TelegramPartHook.Application.Factories;
using TelegramPartHook.Application.Services;
using TelegramPartHook.Domain.Constants;

namespace TelegramPartHook.Application.Queries;

public record GetAdminCommandsQuery 
    : BaseBotRequestCommand
{
    public override string Prefix => "/admin";
}

public class GetAdminCommandsQueryHandler 
    : BaseAdminBotRequestCommandHandler<GetAdminCommandsQuery>
{
    private readonly ITelegramSender _sender;

    public GetAdminCommandsQueryHandler(
        ITelegramSender sender, 
        IAdminConfiguration adminConfiguration,
        ISearchAccessor searchAccessor
    ): base(searchAccessor, adminConfiguration)
    {
        _sender = sender;
    }

    public override async Task Handle(GetAdminCommandsQuery request, CancellationToken cancellationToken)
    {
        var message = new StringBuilder();

        message.AppendLine("*/up telegramid username expiredate* - Upgrade de usuário para VIP. Formato da data: dd/mm/yyyy\n");
        message.AppendLine("*/info telegramid* - Traz a informação do usuário\n");
        message.AppendLine("*/clear ok | cache | error* - Limpa informação. ok = coluna searchesOk de todos os users, cache = Limpa a tabela de cache no BD, error = Limpa a tabela de erro\n");
        message.AppendLine("*/private id1,id2,id3;message* - Envia mensagem privada aos usuarios. Ex: /private 1234,5678,9101112;mensagem para eles\n");
        message.AppendLine("*/newsletterdebug /newslettervip /newsletternonvip /newsletter mensagem* - Envia a newsletter. Pode usar {user} como wildcard pro nome do usuário. {button|msg;/link rogim} para gerar um botão de teclado\n");
        message.AppendLine("*/ad mensagem* - Envia a newsletter. Pode usar {user} como wildcard pro nome do usuário. {button|msg;/link rogim} para gerar um botão de teclado\n");
        message.AppendLine("*/refresh* - Limpa tabela de configuração do bot\n");
        message.AppendLine("*/contributions* - Obtém contribuições enviadas pelos usuários\n");
        message.AppendLine("*/reviewreport* - Obtém a lista de arquivos marcados como não sendo partituras\n");

        await _sender.SendToAdminAsync(message.ToString(), cancellationToken);
    }
}