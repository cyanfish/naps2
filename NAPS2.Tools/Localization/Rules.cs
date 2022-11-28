using System.Text.RegularExpressions;

namespace NAPS2.Tools.Localization;

public static class Rules
{
    private static readonly Regex HotkeyRegex = new Regex(@"&(\w)");
    private static readonly Regex TextPropRegex = new Regex(@"(Text|Items\d*)$");

    public static bool IsTranslatable(bool winforms, string prop, ref string original, out string prefix)
    {
        prefix = "";
        if (!original.Any(char.IsLetter) && !original.Contains("{0}") || original.Length <= 1)
        {
            return false;
        }
        if (winforms)
        {
            if (!TextPropRegex.IsMatch(prop))
            {
                return false;
            }
            var hotkeyMatch = HotkeyRegex.Match(original);
            if (hotkeyMatch.Success)
            {
                prefix = "&";
                original = HotkeyRegex.Replace(original, m => m.Groups[1].Value);
            }
        }
        return true;
    }
}