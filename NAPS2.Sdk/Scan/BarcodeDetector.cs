using NAPS2.Images.Gdi;
using ZXing;
using ZXing.Common;
#if !NETFRAMEWORK
using ZXing.Windows.Compatibility;
#endif

namespace NAPS2.Scan;

/// <summary>
/// A wrapper around the ZXing library that detects patch-t and other barcodes.
/// http://www.alliancegroup.co.uk/patch-codes.htm
/// </summary>
public static class BarcodeDetector
{
    private static readonly BarcodeFormat PATCH_T_FORMAT = BarcodeFormat.CODE_39;

    public static BarcodeDetection Detect(IMemoryImage image, BarcodeDetectionOptions options)
    {
        // TODO: Probably shouldn't have DetectBarcodes be in the options class? The call shouldn't happen at all.
        if (!options.DetectBarcodes)
        {
            return BarcodeDetection.NotAttempted;
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
        // TODO: Needs to be non-windows-specific
        var reader = new BarcodeReader { Options = zxingOptions };
        var result = reader.Decode(gdiImage.Bitmap);
        return new BarcodeDetection(true, result != null, result?.Text);
    }
}