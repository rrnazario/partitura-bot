using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramPartHook.Domain.SeedWork;

public class KeyboardButtons : List<KeyboardButton>
{
    public KeyboardButtons()
    {
        
    }
    
    public KeyboardButtons(IEnumerable<KeyboardButton> buttons) : base(buttons)
    {
    }

    public KeyboardButtons(IEnumerable<(string text, string url)> buttons) : base(buttons.Select(s =>
        new KeyboardButton(s.text, s.url)))
    {
    }

    public KeyboardButtons Add((string text, string url) keyboardButton)
        => Add(new KeyboardButton(keyboardButton.text, keyboardButton.url));

    public new KeyboardButtons Add(KeyboardButton keyboardButton)
    {
        base.Add(keyboardButton);
        return this;
    }
}

public enum KeyboardButtonBreakingLine
{
    NoBreak,
    Before,
    After,
}

public class KeyboardButton
{
    public string Text { get; set; }
    public string Url { get; set; }
    public KeyboardButtonBreakingLine BreakLineType { get; set; }

    public bool IsToBreakLine => BreakLineType == KeyboardButtonBreakingLine.Before ||
                                 BreakLineType == KeyboardButtonBreakingLine.After;


    public KeyboardButton(string text, string url,
        KeyboardButtonBreakingLine breakLine = KeyboardButtonBreakingLine.NoBreak)
    {
        Url = url;
        Text = text;
        BreakLineType = breakLine;
    }

    public InlineKeyboardButton ToInline()
        => Uri.IsWellFormedUriString(Url, UriKind.Absolute)
            ? new InlineKeyboardButton(Text) { Url = Url }
            : new InlineKeyboardButton(Text) { CallbackData = Url };

    public static implicit operator KeyboardButton((string text, string url) button) =>
        new KeyboardButton(button.text, button.url);
}