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
        var alphabet = MapLanguageCodeToAlphabet(languageCode);

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
            // See https://learn.microsoft.com/en-us/typography/fonts/windows_10_font_list
            Alphabet.Bengali => "Nirmala UI",
            Alphabet.CanadianAboriginal => "Gadugi",
            Alphabet.Cherokee => "Gadugi",
            Alphabet.Devanagari => "Nirmala UI",
            Alphabet.Ethiopic => "Ebrima",
            Alphabet.Georgian => "Calibri",
            Alphabet.Gujarati => "Nirmala UI",
            Alphabet.Gurmukhi => "Nirmala UI",
            Alphabet.Kannada => "Nirmala UI",
            Alphabet.Khmer => "Leelawadee UI",
            Alphabet.Lao => "Leelawadee UI",
            Alphabet.Malayalam => "Nirmala UI",
            Alphabet.Myanmar => "Myanmar Text",
            Alphabet.Oriya => "Kalinga", // Supplemental font, needs installation via Control Panel
            Alphabet.Sinhala => "Nirmala UI",
            Alphabet.Syriac => "Estrangelo Edessa", // Supplemental font, needs installation via Control Panel
            Alphabet.Tamil => "Nirmala UI",
            Alphabet.Telugu => "Nirmala UI",
            Alphabet.Thaana => "MV Boli",
            Alphabet.Thai => "Leelawadee UI",
            Alphabet.Tibetan => "Microsoft Himalaya",
            Alphabet.ChineseSimplified => "Microsoft YaHei",
            Alphabet.ChineseTraditional => "Microsoft JhengHei",
            Alphabet.Japanese => "MS Gothic",
            Alphabet.Korean => "Malgun Gothic",
            _ => "Times New Roman"
        };
    }

    private static string GetMacFont(Alphabet alphabet)
    {
        return alphabet switch
        {
            Alphabet.Arabic => ".SF Arabic",
            Alphabet.Armenian => ".SF Armenian",
            Alphabet.Bengali => "Bangla MN",
            Alphabet.CanadianAboriginal => "Euphemia UCAS",
            Alphabet.Cherokee => "Plantagenet Cherokee",
            Alphabet.Devanagari => "Devanagari MT",
            Alphabet.Ethiopic => "Kefa",
            Alphabet.Georgian => ".SF Georgian",
            Alphabet.Gujarati => "Gujarati MT",
            Alphabet.Gurmukhi => "Gurmukhi MT",
            Alphabet.Kannada => "Kannada MN",
            Alphabet.Khmer => "Khmer Sangam MN",
            Alphabet.Lao => "Lao MN",
            Alphabet.Malayalam => "Malayalam MN",
            Alphabet.Myanmar => "Myanmar MN",
            Alphabet.Oriya => "Oriya MN",
            Alphabet.Sinhala => "Sinhala MN",
            Alphabet.Syriac => "Noto Sans Syriac",
            Alphabet.Tamil => "Tamil MN",
            Alphabet.Telugu => "Telugu MN",
            Alphabet.Thaana => "Noto Sans Thaana",
            Alphabet.Thai => "Sathu",
            Alphabet.Tibetan => "Kailasa",
            Alphabet.ChineseSimplified => "Heiti SC",
            Alphabet.ChineseTraditional => "Heiti TC",
            Alphabet.Japanese => ".Aqua Kana",
            Alphabet.Korean => "AppleMyungjo",
            _ => "Times New Roman"
        };
    }

    private static string GetLinuxFont(Alphabet alphabet)
    {
        return alphabet switch
        {
            // Noto fonts aren't always going to be installed, but they're among the most common
            Alphabet.Arabic => "Noto Sans Arabic",
            Alphabet.Armenian => "Noto Sans Armenian",
            Alphabet.Bengali => "Noto Sans Bengali",
            Alphabet.CanadianAboriginal => "Noto Sans CanAborig",
            Alphabet.Cherokee => "Noto Sans Cherokee",
            Alphabet.Devanagari => "Noto Sans Devanagari",
            Alphabet.Ethiopic => "Noto Sans Ethiopic",
            Alphabet.Georgian => "Noto Sans Georgian",
            Alphabet.Gujarati => "Noto Sans Gujarati",
            Alphabet.Gurmukhi => "Noto Sans Gurmukhi",
            Alphabet.Kannada => "Noto Sans Kannada",
            Alphabet.Khmer => "Noto Sans Khmer",
            Alphabet.Lao => "Noto Sans Lao",
            Alphabet.Malayalam => "Noto Sans Malayalam",
            Alphabet.Myanmar => "Noto Sans Myanmar",
            Alphabet.Oriya => "Noto Sans Oriya",
            Alphabet.Sinhala => "Noto Sans Sinhala",
            Alphabet.Syriac => "Noto Sans Syriac",
            Alphabet.Tamil => "Noto Sans Tamil",
            Alphabet.Telugu => "Noto Sans Telugu",
            Alphabet.Thaana => "Noto Sans Thaana",
            Alphabet.Thai => "Noto Sans Thai",
            Alphabet.Tibetan => "Noto Serif Tibetan",
            Alphabet.ChineseSimplified => "Noto Sans CJK SC",
            Alphabet.ChineseTraditional => "Noto Sans CJK TC",
            Alphabet.Japanese => "Noto Sans CJK JP",
            Alphabet.Korean => "Noto Sans CJK KR",
            // Liberation Serif is broadly included in Linux distros and is designed to have the same measurements
            // as Times New Roman.
            _ => "Liberation Serif"
        };
    }

    public enum Alphabet
    {
        Unknown,
        // Common (supported by Times New Roman / Liberation Serif)
        Latin,
        Cyrillic,
        Greek,
        Hebrew,
        // Uncommon (needs special handling)
        Arabic,
        Armenian,
        Bengali,
        CanadianAboriginal,
        Cherokee,
        Devanagari,
        Ethiopic,
        Georgian,
        Gujarati,
        Gurmukhi,
        Kannada,
        Khmer,
        Lao,
        Malayalam,
        Myanmar,
        Oriya,
        Sinhala,
        Syriac,
        Tamil,
        Telugu,
        Thaana,
        Thai,
        Tibetan,
        // CJK (needs special handling)
        ChineseSimplified,
        ChineseTraditional,
        Japanese,
        Korean
    }

    // Reference: https://github.com/tesseract-ocr/langdata_lstm/tree/main/script
    public static Alphabet MapLanguageCodeToAlphabet(string languageCode)
    {
        return languageCode.ToLowerInvariant() switch
        {
            "ara" or "fas" or "kur_ara" or "pus" or "snd" or "uig" or "urd" => Alphabet.Arabic,
            "hye" => Alphabet.Armenian,
            "asm" or "ben" => Alphabet.Bengali,
            "iku" => Alphabet.CanadianAboriginal,
            "chr" => Alphabet.Cherokee,
            "aze_cyrl" or "bel" or "bul" or "kaz" or "kir" or "mkd" or "mon" or "rus" or "srp" or "tgk" or "ukr" or "uzb_cyrl" => Alphabet.Cyrillic,
            "hin" or "mar" or "nep" or "san" => Alphabet.Devanagari,
            "amh" or "tir" => Alphabet.Ethiopic,
            "kat" or "kat_old" => Alphabet.Georgian,
            "ell" or "grc" => Alphabet.Greek,
            "guj" => Alphabet.Gujarati,
            "pan" => Alphabet.Gurmukhi,
            "heb" or "yid" => Alphabet.Hebrew,
            "kan" => Alphabet.Kannada,
            "khm" => Alphabet.Khmer,
            "lao" => Alphabet.Lao,
            "afr" or "aze" or "bos" or "bre" or "cat" or "ceb" or "ces" or "cos" or "cym" or "dan" or "deu" or "eng" or
                "epo" or "est" or "eus" or "fao" or "fil" or "fin" or "fra" or "fry" or "gla" or "gle" or "glg" or
                "hat" or "hrv" or "hun" or "ind" or "isl" or "ita" or "jav" or "lat" or "lav" or "lit" or "ltz" or
                "mlt" or "mri" or "msa" or "nld" or "nor" or "oci" or "pol" or "por" or "que" or "ron" or "slk" or
                "slv" or "spa" or "sqi" or "srp_latn" or "sun" or "swa" or "swe" or "tat" or "ton" or "tur" or "uzb" or
                "yor" => Alphabet.Latin,
            // Tesseract has separate "scripts" for Fraktur and Vietnamese but they both just use Latin characters
            "enm" or "frm" or "deu_latf" or "ita_old" or "spa_old" or "vie" => Alphabet.Latin,
            "mal" => Alphabet.Malayalam,
            "mya" => Alphabet.Myanmar,
            "ori" => Alphabet.Oriya,
            "sin" => Alphabet.Sinhala,
            "syr" => Alphabet.Syriac,
            "tam" => Alphabet.Tamil,
            "tel" => Alphabet.Telugu,
            "div" => Alphabet.Thaana,
            "tha" => Alphabet.Thai,
            "bod" or "dzo" => Alphabet.Tibetan,
            "chi_sim" or "chi_sim_vert" => Alphabet.ChineseSimplified,
            "chi_tra" or "chi_tra_vert" => Alphabet.ChineseTraditional,
            "jpn" or "jpn_vert" => Alphabet.Japanese,
            "kor" or "kor_vert" => Alphabet.Korean,
            _ => Alphabet.Unknown
        };
    }
}