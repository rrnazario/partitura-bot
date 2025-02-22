using MediatR;
using System.Text;
using TelegramPartHook.Application.Commands;
using TelegramPartHook.Application.Factories;
using TelegramPartHook.Application.Services;
using TelegramPartHook.Domain.Aggregations.UserAggregation;
using TelegramPartHook.Domain.Constants;

namespace TelegramPartHook.Application.Queries;

public record GetVIPStatusQuery : BaseBotRequestCommand
{
    public override string Prefix => "/vip";
}

public class GetVIPStatusQueryHandler(
    ITelegramSender sender,
    ISearchAccessor searchAccessor)
    : IRequestHandler<GetVIPStatusQuery>
{
    private readonly User _user = searchAccessor.CurrentSearch().User;

    public async Task Handle(GetVIPStatusQuery request, CancellationToken cancellationToken)
    {
        //TODO: Change it to resource messages!
            
        var message = new StringBuilder("👑 *Olá usuário VIP!*\n\nEis aqui as vantagens disponíveis até agora:\n\n");

        message.AppendLine("📆 */lembretes* - Com esse comando você pode gerenciar seus infinitos lembretes.\n");
        message.AppendLine("📔 */repertorio* - Veja, manipule e gere um PDF do seu repertório com infinitas partituras.\n");
        message.AppendLine("👀 */pdf BUSCA* - Enviando esse comando você já gera automaticamente sua busca em PDF. Basta substituir a palavra _BUSCA_ pelo termo que deseja procurar. Você também consegue baixar as pesquisas clicando no botão que aparece no final da mesma.\n");
        message.AppendLine("🌐 Acesso exclusivo ao portal VIP! Acesse https://www.partituravip.com.br e utilize seu login para o portal nas informações da sua conta 👇");
        message.AppendLine("🌐 No portal VIP você tem todas as vantagens do bot, além de conseguir ver todo o acervo de uma só vez, e montar seu próprio repertório!");
        message.AppendLine("\n\nEm breve muitas outras novidades!");
        message.AppendLine($"\n\nDúvidas? Envie uma mensagem para {AdminConstants.AdminLink}");

        var personalVipMessage = _user.GetVipMessage();

        if (!string.IsNullOrEmpty(personalVipMessage))
        {
            message.AppendLine($"\n\n*Informações da conta:\n*");
            message.AppendLine(personalVipMessage);
        }

        await sender.SendTextMessageAsync(_user.telegramid, message.ToString(), cancellationToken);
    }
}