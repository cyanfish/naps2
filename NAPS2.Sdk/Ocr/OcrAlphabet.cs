namespace NAPS2.Ocr;

/// <summary>
/// The alphabet (script) used by an OCR language. This is used when generating OCR text in exported PDFs, e.g. to avoid
/// adding spaces between words in Chinese/Japanese/Korean text.
/// </summary>
internal enum OcrAlphabet
{
    Unknown,
    // Common
    Latin,
    Cyrillic,
    Greek,
    Hebrew,
    // Uncommon
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
    // CJK
    ChineseSimplified,
    ChineseTraditional,
    Japanese,
    Korean
}

internal static class OcrAlphabetExtensions
{
    extension(OcrAlphabet)
    {
        // Reference: https://github.com/tesseract-ocr/langdata_lstm/tree/main/script
        public static OcrAlphabet FromLanguageCode(string languageCode)
        {
            return languageCode.ToLowerInvariant() switch
            {
                "ara" or "fas" or "kur_ara" or "pus" or "snd" or "uig" or "urd" => OcrAlphabet.Arabic,
                "hye" => OcrAlphabet.Armenian,
                "asm" or "ben" => OcrAlphabet.Bengali,
                "iku" => OcrAlphabet.CanadianAboriginal,
                "chr" => OcrAlphabet.Cherokee,
                "aze_cyrl" or "bel" or "bul" or "kaz" or "kir" or "mkd" or "mon" or "rus" or "srp" or "tgk" or "ukr" or "uzb_cyrl" => OcrAlphabet.Cyrillic,
                "hin" or "mar" or "nep" or "san" => OcrAlphabet.Devanagari,
                "amh" or "tir" => OcrAlphabet.Ethiopic,
                "kat" or "kat_old" => OcrAlphabet.Georgian,
                "ell" or "grc" => OcrAlphabet.Greek,
                "guj" => OcrAlphabet.Gujarati,
                "pan" => OcrAlphabet.Gurmukhi,
                "heb" or "yid" => OcrAlphabet.Hebrew,
                "kan" => OcrAlphabet.Kannada,
                "khm" => OcrAlphabet.Khmer,
                "lao" => OcrAlphabet.Lao,
                "afr" or "aze" or "bos" or "bre" or "cat" or "ceb" or "ces" or "cos" or "cym" or "dan" or "deu" or "eng" or
                    "epo" or "est" or "eus" or "fao" or "fil" or "fin" or "fra" or "fry" or "gla" or "gle" or "glg" or
                    "hat" or "hrv" or "hun" or "ind" or "isl" or "ita" or "jav" or "lat" or "lav" or "lit" or "ltz" or
                    "mlt" or "mri" or "msa" or "nld" or "nor" or "oci" or "pol" or "por" or "que" or "ron" or "slk" or
                    "slv" or "spa" or "sqi" or "srp_latn" or "sun" or "swa" or "swe" or "tat" or "ton" or "tur" or "uzb" or
                    "yor" => OcrAlphabet.Latin,
                // Tesseract has separate "scripts" for Fraktur and Vietnamese but they both just use Latin characters
                "enm" or "frm" or "deu_latf" or "ita_old" or "spa_old" or "vie" => OcrAlphabet.Latin,
                "mal" => OcrAlphabet.Malayalam,
                "mya" => OcrAlphabet.Myanmar,
                "ori" => OcrAlphabet.Oriya,
                "sin" => OcrAlphabet.Sinhala,
                "syr" => OcrAlphabet.Syriac,
                "tam" => OcrAlphabet.Tamil,
                "tel" => OcrAlphabet.Telugu,
                "div" => OcrAlphabet.Thaana,
                "tha" => OcrAlphabet.Thai,
                "bod" or "dzo" => OcrAlphabet.Tibetan,
                "chi_sim" or "chi_sim_vert" => OcrAlphabet.ChineseSimplified,
                "chi_tra" or "chi_tra_vert" => OcrAlphabet.ChineseTraditional,
                "jpn" or "jpn_vert" => OcrAlphabet.Japanese,
                "kor" or "kor_vert" => OcrAlphabet.Korean,
                _ => OcrAlphabet.Unknown
            };
        }
    }
}
