using pix_payload_generator.net.Models.CobrancaModels;
using pix_payload_generator.net.Models.PayloadModels;
using QRCoder;
using TelegramPartHook.Domain.Aggregations.UserAggregation;

namespace TelegramPartHook.Domain.Helpers;

public class PixHelper
{
    public static string GeneratePixString(User user)
    {
        var cobranca = new Cobranca("rrnazario@gmail.com")
        {
            SolicitacaoPagador = $"PartituraVIP {user.fullname} {user.id}",
            Valor = new Valor
            {
                Original = "20.00"
            }
        };

        var payload = cobranca.ToPayload(user.telegramid, new Merchant(user.fullname, "Juiz de Fora"));
        return payload.GenerateStringToQrCode();
    }

    public static string GenerateQrCodeImage(User user)
    {
        var information = GeneratePixString(user);

        return GenerateQrCodeImage(information);
    }

    public static string GenerateQrCodeImage(string information)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(information, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new BitmapByteQRCode(qrCodeData);
        var qrCodeByteArray = qrCode.GetGraphic(20);
        var name = $"{Guid.NewGuid()}.bmp";
        File.WriteAllBytes(name, qrCodeByteArray);

        return name;
    }

        
}