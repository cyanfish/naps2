using NAPS2.Images.Bitwise;
using ZXing;
using ZXing.Common;

namespace NAPS2.Scan;

/// <summary>
/// A wrapper around the ZXing library that detects patch-t and other barcodes.
/// http://www.alliancegroup.co.uk/patch-codes.htm
/// </summary>
internal static class BarcodeDetector
{
    private static readonly BarcodeFormat PATCH_T_FORMAT = BarcodeFormat.CODE_39;

    public static Barcode Detect(IMemoryImage image, BarcodeDetectionOptions options)
    {
        // TODO: Probably shouldn't have DetectBarcodes be in the options class? The call shouldn't happen at all.
        if (!options.DetectBarcodes)
        {
            return Barcode.NoDetection;
        }

        var zxingOptions = options.ZXingOptions ?? new DecodingOptions
        {
            TryHarder = true,
            PossibleFormats = options.PatchTOnly ? [PATCH_T_FORMAT] : null
        };
        var reader = new BarcodeReader<IMemoryImage>(x => new MemoryImageLuminanceSource(x))
        {
             Options = zxingOptions
        };
        var result = reader.Decode(image);
        return new Barcode(true, result != null, result?.Text);
    }
    
    private class MemoryImageLuminanceSource : LuminanceSource
    {
        public MemoryImageLuminanceSource(IMemoryImage image)
            : base(image.Width, image.Height)
        {
            var dstPixelInfo = new PixelInfo(image.Width, image.Height, SubPixelType.Gray);
            var matrix = new byte[dstPixelInfo.Length];
            new CopyBitwiseImageOp().Perform(image, matrix, dstPixelInfo);
            Matrix = matrix;
        }

        public override byte[] getRow(int y, byte[]? row)
        {
            row ??= new byte[Width];
            Array.Copy(Matrix, y * Width, row, 0, Width);
            return row;
        }

        public override byte[] Matrix { get; }
    }
}