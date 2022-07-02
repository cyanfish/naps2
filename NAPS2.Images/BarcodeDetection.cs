using ZXing;

namespace NAPS2.Images;

/// <summary>
/// A wrapper around the ZXing library that detects patch-t and other barcodes.
/// http://www.alliancegroup.co.uk/patch-codes.htm
/// </summary>
// TODO: This should be a generic superclass with just some useful data (e.g. ispatcht).
// TODO: Then a Zxing-specific subclass should be defined in a different project and we can remove the zxing dep here.
public class BarcodeDetection
{
    private const string PATCH_T_TEXT = "PATCHT";

    public static readonly BarcodeDetection NotAttempted = new BarcodeDetection(false, null);

    public BarcodeDetection(bool isAttempted, Result? detectionResult)
    {
        IsAttempted = isAttempted;
        DetectionResult = detectionResult;
    }

    public Result? DetectionResult { get; }

    public bool IsAttempted { get; }

    public bool IsBarcodePresent => DetectionResult != null;

    public bool IsPatchT => DetectionResult?.Text == PATCH_T_TEXT;
}