using ZXing;

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

    /// <summary>
    /// The format of the detected barcode, or null if no barcode was detected.
    /// </summary>
    public BarcodeFormat? DetectedFormat { get; init; }

    /// <summary>
    /// The type of the detected patch code, or null if the detected barcode isn't a patch code.
    /// </summary>
    public PatchCodeType? PatchCode => DetectedText switch
    {
        "PATCH1" => PatchCodeType.PatchI,
        "PATCH2" => PatchCodeType.PatchII,
        "PATCH3" => PatchCodeType.PatchIII,
        "PATCH4" => PatchCodeType.PatchIV,
        "PATCH6" => PatchCodeType.PatchVI,
        PATCH_T_TEXT => PatchCodeType.PatchT,
        _ => null
    };

    public bool IsPatchT => DetectedText == PATCH_T_TEXT;
}