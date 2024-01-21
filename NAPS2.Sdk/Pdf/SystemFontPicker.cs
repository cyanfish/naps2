namespace NAPS2.Pdf;

internal static class SystemFontPicker
{
    public static string GetBestFont(string languageCode)
    {
        var alphabet = languageCode.ToLowerInvariant() switch
        {
            "chi_sim" or "chi_sim_vert" => Alphabet.ChineseSimplified,
            "chi_tra" or "chi_tra_vert" => Alphabet.ChineseTraditional,
            "jpn" or "jpn_vert" => Alphabet.Japanese,
            "kor" or "kor_vert" => Alphabet.Korean,
            _ => Alphabet.Unknown
        };


#if NET6_0_OR_GREATER
        if (OperatingSystem.IsMacOS())
        {
            return GetMacFont(alphabet);
        }
        if (OperatingSystem.IsLinux())
        {
            return GetLinuxFont(alphabet);
        }
#endif
        return GetWindowsFont(alphabet);
    }

    private static string GetWindowsFont(Alphabet alphabet)
    {
        return alphabet switch
        {
            Alphabet.ChineseSimplified => "Microsoft YaHei",
            Alphabet.ChineseTraditional => "Microsoft JhengHei",
            Alphabet.Japanese => "MS Gothic",
            Alphabet.Korean => "Malgun Gothic",
            _ => "Times New Roman"
        };
    }

    private static string GetMacFont(Alphabet alphabet)
    {
        return "Times New Roman";
    }

    private static string GetLinuxFont(Alphabet alphabet)
    {
        // Liberation Serif is broadly included in Linux distros and is designed to have the same measurements
        // as Times New Roman.
        // TODO: Maybe we should use Times New Roman if available?
        return "Liberation Serif";
    }

    public enum Alphabet
    {
        Unknown,
        Latin,
        Arabic,
        Hebrew,
        Cyrillic,
        Greek,
        ChineseSimplified,
        ChineseTraditional,
        Japanese,
        Korean
    }
}