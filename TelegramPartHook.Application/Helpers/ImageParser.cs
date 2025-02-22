using System.Text.RegularExpressions;

namespace TelegramPartHook.Application.Helpers;

public static partial class ImageParser
{
    public static MemoryStream FromBase64ToStream(string imageBase64)
    {
        var bytes = FromBase64(imageBase64);
        var contents = new MemoryStream(bytes);
        contents.Position = 0;

        return contents;
    }

    private static byte[] FromBase64(string imageBase64)
    {
        var regex = ContentInvalidCharsRemovalRegex();
        return Convert.FromBase64String(regex.Replace(imageBase64, string.Empty));
    }

    [GeneratedRegex("data\\:[\\/a-z]+;base64,")]
    private static partial Regex ContentInvalidCharsRemovalRegex();
}