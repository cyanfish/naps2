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

    public BarcodeFormat? DetectedFormat { get; init; }

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

public enum PatchCodeType
{
    PatchI,
    PatchII,
    PatchIII,
    PatchIV,
    PatchVI,
    PatchT
}