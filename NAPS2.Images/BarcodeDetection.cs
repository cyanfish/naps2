namespace NAPS2.Images;

/// <summary>
/// A wrapper around the ZXing library that detects patch-t and other barcodes.
/// http://www.alliancegroup.co.uk/patch-codes.htm
/// </summary>
public class BarcodeDetection
{
    private const string PATCH_T_TEXT = "PATCHT";

    public static readonly BarcodeDetection NotAttempted = new BarcodeDetection(false, false, null);

    public BarcodeDetection(bool isAttempted, bool isBarcodePresent, string? detectedText)
    {
        IsAttempted = isAttempted;
        IsBarcodePresent = isBarcodePresent;
        DetectedText = detectedText;
    }

    private BarcodeDetection()
    {
    }

    public string? DetectedText { get; }

    public bool IsAttempted { get; }

    public bool IsBarcodePresent { get; }

    public bool IsPatchT => DetectedText == PATCH_T_TEXT;
}