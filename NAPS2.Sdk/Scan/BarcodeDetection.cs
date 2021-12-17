using ZXing;
using ZXing.Common;

namespace NAPS2.Scan;

/// <summary>
/// A wrapper around the ZXing library that detects patch-t and other barcodes.
/// http://www.alliancegroup.co.uk/patch-codes.htm
/// </summary>
public class BarcodeDetection
{
    private const string PATCH_T_TEXT = "PATCHT";
    private static readonly BarcodeFormat PATCH_T_FORMAT = BarcodeFormat.CODE_39;
        
    public static readonly BarcodeDetection NotAttempted = new BarcodeDetection(false, null);
        
    public static BarcodeDetection Detect(IImage image, BarcodeDetectionOptions options)
    {
        if (!options.DetectBarcodes)
        {
            return NotAttempted;
        }
        // TODO: Make more generic
        if (!(image is GdiImage gdiImage))
        {
            throw new InvalidOperationException("Patch code detection only supported for GdiStorage");
        }

        var zxingOptions = options.ZXingOptions ?? new DecodingOptions
        {
            TryHarder = true,
            PossibleFormats = options.PatchTOnly ? new List<BarcodeFormat> { PATCH_T_FORMAT } : null
        };
        var reader = new BarcodeReader { Options = zxingOptions };
        return new BarcodeDetection(true, reader.Decode(gdiImage.Bitmap));
    }
        
    private BarcodeDetection(bool isAttempted, Result? detectionResult)
    {
        IsAttempted = isAttempted;
        DetectionResult = detectionResult;
    }

    public Result? DetectionResult { get; }
        
    public bool IsAttempted { get; }

    public bool IsBarcodePresent => DetectionResult != null;

    public bool IsPatchT => DetectionResult?.Text == PATCH_T_TEXT;
}