namespace Dsp.WebCore.Extensions;
using System.Globalization;

public static class StringExtensions
{
    public static string ToTitleCaseString(this string original)
    {
        var formattedText = string.Empty;

        var words = original.Split(' ');
        for (var i = 0; i < words.Length; i++)
        {
            if (!words[i].IsAllUpper() && !char.IsNumber(words[i][0]))
            {
                words[i] = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(words[i].ToLowerInvariant());
            }
            formattedText += words[i];
            if (i < words.Length - 1)
                formattedText += " ";
        }

        return formattedText;
    }

    public static bool IsAllUpper(this string input)
    {
        for (var i = 0; i < input.Length; i++)
        {
            if (char.IsLetter(input[i]) && !char.IsUpper(input[i]))
                return false;
        }
        return true;
    }
}
