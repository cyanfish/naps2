using NAPS2.Images.Bitwise;
using ZXing;
using ZXing.Common;

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

        var zxingOptions = options.ZXingOptions ?? new DecodingOptions
        {
            TryHarder = true,
            PossibleFormats = options.PatchTOnly ? new List<BarcodeFormat> { PATCH_T_FORMAT } : null
        };
        var reader = new BarcodeReader<IMemoryImage>(x => new MemoryImageLuminanceSource(x))
        {
             Options = zxingOptions
        };
        var result = reader.Decode(image);
        return new BarcodeDetection(true, result != null, result?.Text);
    }
    
    private class MemoryImageLuminanceSource : LuminanceSource
    {
        public unsafe MemoryImageLuminanceSource(IMemoryImage image)
            : base(image.Width, image.Height)
        {
            var matrix = new byte[image.Width * image.Height];
            fixed (byte* ptr = &matrix[0])
            {
                var dstPix = PixelInfo.Gray(ptr, image.Width, image.Width, image.Height);
                new CopyBitwiseImageOp().Perform(image, dstPix);
            }
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