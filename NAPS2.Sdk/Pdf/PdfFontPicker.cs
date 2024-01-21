namespace NAPS2.Pdf;

/// <summary>
/// Determines the best font to use for generating OCR text in exported PDFs. As this text is invisible, the quality
/// and style of the font aren't so important - what matters is that the font is installed on the system by default and
/// supports the characters in the current language's alphabet.
/// </summary>
internal static class PdfFontPicker
{
    public static string GetBestFont(string languageCode)
    {
        // This logic is incomplete, but the goal is to get PdfFontTests passing, which works as
        // the default font supports multiple scripts. e.g. Times New Roman supports most everything
        // except CJK (Chinese-Japanese-Korean)
        var alphabet = languageCode.ToLowerInvariant() switch
        {
            "chi_sim" or "chi_sim_vert" => Alphabet.ChineseSimplified,
            "chi_tra" or "chi_tra_vert" => Alphabet.ChineseTraditional,
            "jpn" or "jpn_vert" => Alphabet.Japanese,
            "kor" or "kor_vert" => Alphabet.Korean,
            "ara" or "fas" or "msa" or "snd" or "urd" => Alphabet.Arabic,
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
        return alphabet switch
        {
            // Noto fonts aren't always going to be installed, but they're among the most common
            Alphabet.ChineseSimplified => "Noto Sans CJK SC",
            Alphabet.ChineseTraditional => "Noto Sans CJK TC",
            Alphabet.Japanese => "Noto Sans CJK JP",
            Alphabet.Korean => "Noto Sans CJK KR",
            Alphabet.Arabic => "Noto Sans Arabic",
            // Liberation Serif is broadly included in Linux distros and is designed to have the same measurements
            // as Times New Roman.
            // TODO: Maybe we should use Times New Roman if available?
            _ => "Liberation Serif"
        };
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