namespace NAPS2.Ocr;

// TODO: Maybe should separate this for the SDK and for config
/// <summary>
/// Configuration parameters for an OCR request.
///
/// For language codes, see
/// https://tesseract-ocr.github.io/tessdoc/Data-Files#data-files-for-version-400-november-29-2016
/// </summary>
public record OcrParams(string? LanguageCode, OcrMode Mode = OcrMode.Default, double TimeoutInSeconds = 0)
{
    private OcrParams()
        : this(null, OcrMode.Default, 0)
    {
    }
    
    public static readonly OcrParams Empty = new();
}