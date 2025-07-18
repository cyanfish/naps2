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
    private static readonly ZXing.BarcodeFormat PATCH_T_FORMAT = ZXing.BarcodeFormat.CODE_39;

    public static Barcode Detect(IMemoryImage image, BarcodeDetectionOptions options)
    {
        // TODO: Probably shouldn't have DetectBarcodes be in the options class? The call shouldn't happen at all.
        if (!options.DetectBarcodes)
        {
            return Barcode.NoDetection;
        }

        // Create a ZXing DecodingOptions object based on the provided options.
        var zxingOptions = new DecodingOptions
        {
            TryHarder = true,
            PureBarcode = options.PureBarcode,
            CharacterSet = options.CharacterSet,
            UseCode39ExtendedMode = options.UseCode39ExtendedMode,
            UseCode39RelaxedExtendedMode = options.UseCode39RelaxedExtendedMode,
            AssumeCode39CheckDigit = options.AssumeCode39CheckDigit,
            ReturnCodabarStartEnd = options.ReturnCodabarStartEnd,
            AssumeGS1 = options.AssumeGS1,
            AssumeMSICheckDigit = options.AssumeMSICheckDigit,
            AllowedLengths = options.AllowedLengths,
            AllowedEANExtensions = options.AllowedEANExtensions
        };
        if (options.PatchTOnly)
        {
            zxingOptions.PossibleFormats = [PATCH_T_FORMAT];
        }
        else
        {
            foreach (ZXing.BarcodeFormat format in Enum.GetValues(typeof(ZXing.BarcodeFormat)))
            {
#pragma warning disable CA2248 // Provide correct 'enum' argument to 'Enum.HasFlag'
                if (options.PossibleFormats.HasFlag(format))
                {
                    zxingOptions.PossibleFormats ??= [];
                    zxingOptions.PossibleFormats.Add(format);
                }
#pragma warning restore CA2248 // Provide correct 'enum' argument to 'Enum.HasFlag'
            }
        }
 
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