using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TelegramPartHook.Infrastructure.Helpers;

public static class StringHelper
{
    /// <summary>
    /// Replace any diacritic char by it non-diacritic form.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string ReplaceDiacritics(this string input)
    {
        var inputInFormD = input.Normalize(NormalizationForm.FormD);

        var output = new StringBuilder();
        foreach (char c in inputInFormD)
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                output.Append(c);

        return output.ToString().Normalize(NormalizationForm.FormC);
    }

    public static bool ContainsDiacritics(this string input)
        => !input.ReplaceDiacritics().Equals(input);

    /// <summary>
    /// Replace spaces by plus signals to be placed at query statement.
    /// </summary>
    /// <param name="term"></param>
    /// <returns></returns>
    public static string AdjustSearch(this string term) => term.Replace(" ", "+");

    public static string AdjustSearchLabel(this string term) => term.Capitalize().Replace(" ", "%20");

    /// <summary>
    /// This method remove invalid chars, capitalize each word and remove multiple spaces in a given input string.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string WellFormat(this string input) => input.RemoveInvalidChars().CapitalizeEachWord()
        .RemoveMultipleSpaces().ReplaceDiacritics();

    /// <summary>
    /// Remove multiples spaces that occurs on a given string.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string RemoveMultipleSpaces(this string input)
    {
        var regex = new Regex("[ ]{2,}", RegexOptions.None);
        input = regex.Replace(input, " ").Replace("\n", " ");
        input = input.TrimEnd().TrimStart().Trim();

        return input;
    }

    /// <summary>
    /// Remove invalid chars from a given string.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="keepNumbers"></param>
    /// <returns></returns>
    public static string RemoveInvalidChars(this string input, bool keepNumbers = false)
    {
        var invalidChars = new string[] { ".", "*", "-", "," };
        string returnString;

        returnString = input.Contains(":") ? input.Split(':').Last() : input;

        foreach (var item in invalidChars)
            returnString = returnString.Replace(item, string.Empty);

        return new string((!keepNumbers
            ? returnString.Where(w => !char.IsDigit(w))
            : returnString).ToArray());
    }

    /// <summary>
    /// Put the first letter of a word in UpperCase.
    /// </summary>
    /// <param name="word"></param>
    /// <returns></returns>
    private static string Capitalize(this string word)
    {
        var split = word.Split(' ');

        if (split.Length > 0)
            word = word.Replace(split[0], new CultureInfo("pt-br").TextInfo.ToTitleCase(split[0]));

        return word;
    }

    public static string RemoveInvalidFileNameChars(this string filename)
        => string.Concat(filename.Split(Path.GetInvalidFileNameChars()));

    /// <summary>
    /// Capitalizes every word that contains in a given sentence.
    /// </summary>
    /// <param name="sentence"></param>
    /// <returns></returns>
    private static string CapitalizeEachWord(this string sentence)
    {
        var result = string.Empty;

        sentence.ToLowerInvariant().Split(' ').ToList().ForEach(aWord =>
        {
            result += string.IsNullOrEmpty(result)
                ? aWord.Capitalize()
                : string.Concat(" ", aWord.Capitalize());
        });

        return result;
    }

    public static bool IsValidJson(this string input)
    {
        try
        {
            var x = JToken.Parse(input);
            return x is not null;
        }
        catch
        {
            return false;
        }
    }

    public static string GetUserTelegramIdFromRequest(string request)
        => GetUserInfoFromRequest(request, "id");

    public static string GetCultureFromRequest(string request)
        => GetUserInfoFromRequest(request, "language_code");

    private static string GetUserInfoFromRequest(string request, string info)
    {
        var dynamicContent = (dynamic)JsonConvert.DeserializeObject(request);

        try
        {
            return (string)dynamicContent["originalDetectIntentRequest"]["payload"]["data"]["from"][info];
        }
        catch //Callback
        {
            return (string)dynamicContent["originalDetectIntentRequest"]["payload"]["data"]["callback_query"]
                ["from"][info];
        }
    }
}