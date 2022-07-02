namespace NAPS2.Ocr;

// TODO: Maybe should separate this for the SDK and for config
/// <summary>
/// Configuration parameters for an OCR request.
///
/// For language codes, see
/// https://tesseract-ocr.github.io/tessdoc/Data-Files#data-files-for-version-400-november-29-2016
/// </summary>
public record OcrParams(string? LanguageCode, OcrMode Mode, double TimeoutInSeconds)
{
    public static readonly OcrParams Empty = new(null, OcrMode.Default, 0);
}