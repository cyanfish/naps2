using NAPS2.Images.Transforms;
using NAPS2.Scan;
using NAPS2.Serialization;
using ZXing;
using Xunit;

namespace NAPS2.Sdk.Tests.Images;

public class BarcodeTests : ContextualTests
{
    // TODO: Also add unit/integration tests for scan/import to ensure things are set correctly (and don't malfunction if detection is off...)
    // TODO: Also add unit/integration tests for patch-t splitting for batch/commandline etc, to ensure detection is enabled and splitting works

    [Fact]
    public void DetectPatchT()
    {
        var image = LoadImage(ImageResources.patcht);
        var barcode = BarcodeDetector.Detect(image, new BarcodeDetectionOptions
        {
            DetectBarcodes = true,
            PatchTOnly = true
        });
        Assert.True(barcode.IsDetectionAttempted);
        Assert.True(barcode.IsDetected);
        Assert.True(barcode.IsPatchT);
    }

    [Fact]
    public void DetectUpc()
    {
        var image = LoadImage(ImageResources.image_upc_barcode);
        var barcode = BarcodeDetector.Detect(image, new BarcodeDetectionOptions
        {
            DetectBarcodes = true,
            PatchTOnly = false
        });
        Assert.True(barcode.IsDetectionAttempted);
        Assert.True(barcode.IsDetected);
        Assert.False(barcode.IsPatchT);
        Assert.Equal("725272730706", barcode.DetectedText);
        Assert.Equal(BarcodeFormat.UPC_A, barcode.DetectedFormat);
        Assert.Equal(BarcodeFormat.UPC_A, barcode.ToXml().FromXml<Barcode>().DetectedFormat);
    }

    [Theory]
    [InlineData(150, 60, 166, 846, "ABC-abc-1234", BarcodeFormat.CODE_128)]
    [InlineData(150, 190, 166, 716, "ABC-1234", BarcodeFormat.CODE_39)]
    [InlineData(320, 320, 316, 596, "This is a QR Code by TEC-IT", BarcodeFormat.QR_CODE)]
    [InlineData(280, 450, 286, 476, "725272730706", BarcodeFormat.UPC_A)]
    [InlineData(280, 570, 286, 356, "9780201379624", BarcodeFormat.EAN_13)]
    public void DetectBarcodeSamples(int left, int top, int right, int bottom, string text, BarcodeFormat format)
    {
        var barcode = DetectBarcode(left, top, right, bottom);

        Assert.True(barcode.IsDetectionAttempted);
        Assert.True(barcode.IsDetected);
        Assert.Equal(text, barcode.DetectedText);
        Assert.Equal(format, barcode.DetectedFormat);
    }

    [Fact]
    public void DetectCode128AndCode39WithFormatRestrictions()
    {
        var code128 = DetectBarcode(150, 60, 166, 846, BarcodeFormat.CODE_128);
        var code39 = DetectBarcode(150, 190, 166, 716, BarcodeFormat.CODE_39);
        var code128AsCode39 = DetectBarcode(150, 60, 166, 846, BarcodeFormat.CODE_39);
        var code39AsCode128 = DetectBarcode(150, 190, 166, 716, BarcodeFormat.CODE_128);

        Assert.Equal("ABC-abc-1234", code128.DetectedText);
        Assert.Equal(BarcodeFormat.CODE_128, code128.DetectedFormat);
        Assert.Equal("ABC-1234", code39.DetectedText);
        Assert.Equal(BarcodeFormat.CODE_39, code39.DetectedFormat);
        Assert.False(code128AsCode39.IsDetected);
        Assert.False(code39AsCode128.IsDetected);
    }

    private Barcode DetectBarcode(int left, int top, int right, int bottom, BarcodeFormat? format = null)
    {
        using var image = LoadImage(ImageResources.barcodes);
        using var crop = image.PerformTransform(new CropTransform(left, right, top, bottom));
        return BarcodeDetector.Detect(crop, new BarcodeDetectionOptions
        {
            DetectBarcodes = true,
            PatchTOnly = false,
            ZXingOptions = format == null ? null : new ZXing.Common.DecodingOptions
            {
                TryHarder = true,
                PossibleFormats = [format.Value]
            }
        });
    }

    [Fact]
    public void DetectNothing()
    {
        var image = LoadImage(ImageResources.dog);
        var barcode = BarcodeDetector.Detect(image, new BarcodeDetectionOptions
        {
            DetectBarcodes = true,
            PatchTOnly = false
        });
        Assert.True(barcode.IsDetectionAttempted);
        Assert.False(barcode.IsDetected);
        Assert.False(barcode.IsPatchT);
        Assert.Null(barcode.DetectedText);
    }
}