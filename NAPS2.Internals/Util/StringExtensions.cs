namespace NAPS2.Util;

public static class StringExtensions
{
    public static bool ContainsInvariantIgnoreCase(this string source, string value)
    {
        return source.IndexOf(value, StringComparison.InvariantCultureIgnoreCase) != -1;
    }
}