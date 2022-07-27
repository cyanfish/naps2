using NAPS2.Images.Gdi;
using NAPS2.ImportExport.Images;
using NAPS2.Scan;
using NAPS2.Sdk.Tests.Asserts;
using Xunit;

namespace NAPS2.Sdk.Tests.ImportExport;

public class ImportPostProcessorTests : ContextualTests
{
    private readonly ImportPostProcessor _importPostProcessor;

    public ImportPostProcessorTests()
    {
        _importPostProcessor = new ImportPostProcessor(ImageContext);
    }

    [Fact]
    public void NoPostProcessing()
    {
        using var image = ScanningContext.CreateProcessedImage(new GdiImage(ImageResources.color_image));
        using var image2 =
            _importPostProcessor.AddPostProcessingData(image, null, null, new BarcodeDetectionOptions(), false);

        Assert.Null(image2.PostProcessingData.Thumbnail);
        Assert.Null(image2.PostProcessingData.ThumbnailTransformState);
        Assert.False(image2.PostProcessingData.BarcodeDetection.IsAttempted);
        Assert.False(IsDisposed(image2));
        image2.Dispose();
        Assert.False(IsDisposed(image));
    }

    [Fact]
    public void DisposesOriginalImageWithNoPostProcessing()
    {
        using var image = ScanningContext.CreateProcessedImage(new GdiImage(ImageResources.color_image));
        using var image2 =
            _importPostProcessor.AddPostProcessingData(image, null, null, new BarcodeDetectionOptions(), true);

        Assert.False(IsDisposed(image2));
        image2.Dispose();
        Assert.True(IsDisposed(image));
    }

    [Fact]
    public void ThumbnailRendering()
    {
        using var image = ScanningContext.CreateProcessedImage(new GdiImage(ImageResources.color_image));
        using var image2 =
            _importPostProcessor.AddPostProcessingData(image, null, 256, new BarcodeDetectionOptions(), false);

        var actual = image2.PostProcessingData.Thumbnail;

        Assert.NotNull(actual);
        Assert.NotNull(image2.PostProcessingData.ThumbnailTransformState);
        Assert.True(image2.PostProcessingData.ThumbnailTransformState.IsEmpty);
        ImageAsserts.Similar(ImageResources.color_image_thumb_256, actual);
    }

    [Fact]
    public void ThumbnailRenderingWithTransform()
    {
        using var image = ScanningContext.CreateProcessedImage(new GdiImage(ImageResources.color_image));
        using var image2 = image.WithTransform(new BrightnessTransform(300));
        using var image3 =
            _importPostProcessor.AddPostProcessingData(image2, null, 256, new BarcodeDetectionOptions(), false);

        var actual = image3.PostProcessingData.Thumbnail;

        Assert.NotNull(actual);
        Assert.NotNull(image3.PostProcessingData.ThumbnailTransformState);
        Assert.Single(image3.PostProcessingData.ThumbnailTransformState.Transforms);
        var transform =
            Assert.IsType<BrightnessTransform>(image3.PostProcessingData.ThumbnailTransformState.Transforms[0]);
        Assert.Equal(300, transform.Brightness);
        ImageAsserts.Similar(ImageResources.color_image_b_p300_thumb_256, actual);
    }

    [Fact]
    public void ThumbnailRenderingWithPrerenderedImageAndDisposingOriginal()
    {
        using var rendered = new GdiImage(ImageResources.color_image);
        using var image = ScanningContext.CreateProcessedImage(rendered);
        using var image2 =
            _importPostProcessor.AddPostProcessingData(image, rendered, 256, new BarcodeDetectionOptions(), true);

        var actual = image2.PostProcessingData.Thumbnail;

        Assert.NotNull(actual);
        Assert.NotNull(image2.PostProcessingData.ThumbnailTransformState);
        Assert.True(image2.PostProcessingData.ThumbnailTransformState.IsEmpty);
        ImageAsserts.Similar(ImageResources.color_image_thumb_256, actual);
        Assert.False(IsDisposed(rendered));
        Assert.False(IsDisposed(image2));
        image2.Dispose();
        Assert.True(IsDisposed(image));
    }

    [Fact]
    public void BarcodeDetection()
    {
        using var image = ScanningContext.CreateProcessedImage(new GdiImage(ImageResources.patcht));
        var barcodeOptions = new BarcodeDetectionOptions { DetectBarcodes = true };
        using var image2 = _importPostProcessor.AddPostProcessingData(image, null, null, barcodeOptions, false);

        Assert.True(image2.PostProcessingData.BarcodeDetection.IsPatchT);
    }

    [Fact]
    public void BarcodeDetectionWithPrerenderedImage()
    {
        using var rendered = new GdiImage(ImageResources.patcht);
        using var image = ScanningContext.CreateProcessedImage(rendered);
        var barcodeOptions = new BarcodeDetectionOptions { DetectBarcodes = true };
        using var image2 = _importPostProcessor.AddPostProcessingData(image, rendered, null, barcodeOptions, false);

        Assert.True(image2.PostProcessingData.BarcodeDetection.IsPatchT);
    }
}