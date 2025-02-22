using System.Text.RegularExpressions;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramPartHook.Domain.SeedWork;
using KeyboardButton = TelegramPartHook.Domain.SeedWork.KeyboardButton;

namespace TelegramPartHook.Application.Helpers;


public static class TelegramHelper
{
    public static InlineKeyboardMarkup? GenerateKeyboard(KeyboardButtons buttons, int columnNumber = 1)
    {
        if (!buttons.Any()) return null;
        if (columnNumber > 8) throw new ArgumentException("Max columnNumber accepted is 8");

        var inlineButtons = new List<List<InlineKeyboardButton>>();
        var currentLine = new List<InlineKeyboardButton>();
        foreach (var button in buttons)
        {
            var inlineButton = button.ToInline();

            if (button.BreakLineType == KeyboardButtonBreakingLine.NoBreak ||
                button.BreakLineType == KeyboardButtonBreakingLine.After)
                currentLine.Add(inlineButton);

            if (currentLine.Count == columnNumber || button.IsToBreakLine)
            {
                inlineButtons.Add(currentLine);
                currentLine = new();

                if (button.BreakLineType == KeyboardButtonBreakingLine.Before)
                {
                    currentLine.Add(inlineButton);
                }
            }
        }

        if (currentLine.Any() && currentLine.Count < columnNumber)
        {
            inlineButtons.Add(currentLine);
        }

        return new InlineKeyboardMarkup(inlineButtons);
    }

    public static InlineKeyboardMarkup? GenerateTrueFalseKeyboard(string placeholder)
    {
        var buttons = new KeyboardButton[]
        {
            new("✅", $"{placeholder}true"),
            new("❌", $"{placeholder}false")
        };
        return GenerateKeyboard(new(buttons));
    }

    public static InlineKeyboardMarkup? TryGenerateKeyboard(ref string msg)
    {
        var regexButtons = Regex.Matches(msg, @"\{button\|[a-zA-Z\s;,\/\s\.\:\-]+\}");

        var buttons = Array.Empty<(string caption, string url)>();
        if (regexButtons.Any())
        {
            buttons = regexButtons
                .Select(s => s.Value.Split("|").Last().Split(";")) //{button|msg;/link rogim}
                .Select(s => (caption: s.First(), url: s.Last().Replace("}", "")))
                .ToArray();

            foreach (Match item in regexButtons)
                msg = msg.Replace(item.Value, string.Empty);

            msg = msg.Trim();
        }

        return GenerateKeyboard(new(buttons));
    }
}