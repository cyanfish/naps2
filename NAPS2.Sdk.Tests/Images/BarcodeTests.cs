using NAPS2.Images.Gdi;
using NAPS2.Scan;
using Xunit;

namespace NAPS2.Sdk.Tests.Images;

public class BarcodeTests
{
    // TODO: Also add unit/integration tests for scan/import to ensure things are set correctly (and don't malfunction if detection is off...)
    // TODO: Also add unit/integration tests for patch-t splitting for batch/commandline etc, to ensure detection is enabled and splitting works

    [Fact]
    public void DetectPatchT()
    {
        var image = new GdiImage(SharedData.patcht);
        var detection = BarcodeDetector.Detect(image, new BarcodeDetectionOptions
        {
            DetectBarcodes = true,
            PatchTOnly = true
        });
        Assert.True(detection.IsAttempted);
        Assert.True(detection.IsBarcodePresent);
        Assert.True(detection.IsPatchT);
    }

    [Fact]
    public void DetectUpc()
    {
        var image = new GdiImage(BarcodeTestsData.image_upc_barcode);
        var detection = BarcodeDetector.Detect(image, new BarcodeDetectionOptions
        {
            DetectBarcodes = true,
            PatchTOnly = false
        });
        Assert.True(detection.IsAttempted);
        Assert.True(detection.IsBarcodePresent);
        Assert.False(detection.IsPatchT);
        Assert.Equal("725272730706", detection.DetectedText);
    }

    [Fact]
    public void DetectNothing()
    {
        var image = new GdiImage(SharedData.color_image);
        var detection = BarcodeDetector.Detect(image, new BarcodeDetectionOptions
        {
            DetectBarcodes = true,
            PatchTOnly = false
        });
        Assert.True(detection.IsAttempted);
        Assert.False(detection.IsBarcodePresent);
        Assert.False(detection.IsPatchT);
        Assert.Null(detection.DetectedText);
    }
}