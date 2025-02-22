using Telegram.Bot.Types.ReplyMarkups;
using TelegramPartHook.Application.Helpers;
using KeyboardButton = TelegramPartHook.Domain.SeedWork.KeyboardButton;

namespace TelegramPartHook.Application.Commands.Repertoire
{
    internal static class RepertoireHelper
    {
        public static InlineKeyboardMarkup GenerateActionKeyboard()
        {
            var buttons = new List<KeyboardButton>
            {
                new("Ver repertório", SeeRepertoireViaTelegramCommandPrefix),
                new("Limpar", CleanRepertoireViaTelegramCommandPrefix),
                new("Gerar PDF", GeneratePDFRepertoireViaTelegramCommandPrefix)
            };

            return TelegramHelper.GenerateKeyboard(new(buttons))!;
        }

        public const string GeneratePDFRepertoireViaTelegramCommandPrefix = "/imprimirrepertorio";
        public const string CleanRepertoireViaTelegramCommandPrefix = "/limparrepertorio";
        public const string AddRepertoireViaTelegramCommandPrefix = "/addrepertoiretg";
        public const string RemoveFromRepertoireViaTelegramCommandPrefix = "/removerrepertorio";
        public const string SeeRepertoireViaTelegramCommandPrefix = "/repertorio";
        public const string AddRepertoireByImageViaTelegramCommandPrefix = "/repertoireimage";
        public const string RemoveRepertoireByImageViaTelegramCommandPrefix = "/repertoireremove";
    }
}
