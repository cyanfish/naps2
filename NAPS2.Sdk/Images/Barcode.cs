namespace NAPS2.Images;

/// <summary>
/// A wrapper around the ZXing library that detects patch-t and other barcodes.
/// http://www.alliancegroup.co.uk/patch-codes.htm
/// </summary>
public record Barcode(bool IsDetectionAttempted, bool IsDetected, string? DetectedText)
{
    private const string PATCH_T_TEXT = "PATCHT";

    public static readonly Barcode NoDetection = new(false, false, null);

    private Barcode() : this(false, false, null)
    {
    }

    public bool IsPatchT => DetectedText == PATCH_T_TEXT;
}