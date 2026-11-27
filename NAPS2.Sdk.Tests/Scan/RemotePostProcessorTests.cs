using NAPS2.Ocr;
using NAPS2.Scan;
using NAPS2.Scan.Internal;
using NAPS2.Sdk.Tests.Asserts;
using Xunit;

namespace NAPS2.Sdk.Tests.Scan;

public class RemotePostProcessorTests : ContextualTests
{
    private readonly RemotePostProcessor _remotePostProcessor;

    public RemotePostProcessorTests()
    {
        _remotePostProcessor = new RemotePostProcessor(ScanningContext);
    }

    [Fact]
    public void Blank()
    {
        var image = LoadImage(ImageResources.blank1);
        var options = new ScanOptions
        {
            ExcludeBlankPages = true
        };
        var result = _remotePostProcessor.PostProcess(image, options, new PostProcessingContext());
        Assert.Null(result);
    }

    [Fact]
    public void NotBlank()
    {
        var image = LoadImage(ImageResources.notblank);
        var options = new ScanOptions
        {
            ExcludeBlankPages = true
        };
        var result = _remotePostProcessor.PostProcess(image, options, new PostProcessingContext());
        Assert.NotNull(result);
    }

    [Fact]
    public void Brightness()
    {
        var image = LoadImage(ImageResources.dog);
        var options = new ScanOptions
        {
            Brightness = 300,
            ThumbnailSize = 256
        };
        var result = _remotePostProcessor.PostProcess(image, options, new PostProcessingContext());
        Assert.Single(result.TransformState.Transforms);
        Assert.Equal(300, Assert.IsType<BrightnessTransform>(result.TransformState.Transforms[0]).Brightness);
        ImageAsserts.Similar(ImageResources.dog_b_p300_thumb_256, result.PostProcessingData.Thumbnail, ignoreResolution: true, rmseThreshold: ImageAsserts.XPLAT_RMSE_THRESHOLD);
    }

    [Fact]
    public void AutoDeskew()
    {
        var image = LoadImage(ImageResources.skewed);
        var options = new ScanOptions
        {
            AutoDeskew = true
        };
        var result = _remotePostProcessor.PostProcess(image, options, new PostProcessingContext());
        ImageAsserts.Similar(ImageResources.deskewed, result, ImageAsserts.XL_RMSE_THRESHOLD);
    }

    [Fact]
    public void AutoCrop()
    {
        var image = LoadImage(ImageResources.autocrop_doc);
        var options = new ScanOptions
        {
            AutoCrop = true
        };
        var result = _remotePostProcessor.PostProcess(image, options, new PostProcessingContext());
        using var rendered = result!.Render();
        // The page fills the width of the scan, with some scanner background at the bottom
        Assert.Equal(2550, rendered.Width);
        const int expectedHeight = 3318;
        const int autoCropTolerance = 20;
        Assert.InRange(rendered.Height, expectedHeight - autoCropTolerance, expectedHeight + autoCropTolerance);
    }

    [Fact]
    public void DocumentCorrection()
    {
        var image = LoadImage(ImageResources.dog);
        var options = new ScanOptions
        {
            CorrectionMode = CorrectionMode.Document
        };
        var result = _remotePostProcessor.PostProcess(image, options, new PostProcessingContext());
        Assert.Contains(new CorrectionTransform(CorrectionMode.Document), result!.TransformState.Transforms);
    }

    [Fact]
    public void Rotate()
    {
        var image = LoadImage(ImageResources.dog);
        var options = new ScanOptions
        {
            RotateDegrees = 90
        };
        var result = _remotePostProcessor.PostProcess(image, options, new PostProcessingContext());
        ImageAsserts.Similar(ImageResources.dog_r_p90, result, ImageAsserts.XPLAT_RMSE_THRESHOLD);
    }

    [Fact]
    public void CropToPageSize_BothPortrait()
    {
        var image = LoadImage(ImageResources.patcht);
        var options = new ScanOptions
        {
            CropToPageSize = true,
            PageSize = new PageSize(8m, 10m, PageSizeUnit.Inch)
        };
        var result = _remotePostProcessor.PostProcess(image, options, new PostProcessingContext());
        ImageAsserts.Similar(ImageResources.patcht_cropped_br, result);
    }

    [Fact]
    public void CropToPageSize_SizeLandscape()
    {
        var image = LoadImage(ImageResources.patcht);
        var options = new ScanOptions
        {
            CropToPageSize = true,
            PageSize = new PageSize(10m, 8m, PageSizeUnit.Inch)
        };
        var result = _remotePostProcessor.PostProcess(image, options, new PostProcessingContext());
        ImageAsserts.Similar(ImageResources.patcht_cropped_br, result);
    }

    [Fact]
    public void CropToPageSize_ImageLandscape()
    {
        var image = LoadImage(ImageResources.patcht).PerformTransform(new RotationTransform(-90));
        var options = new ScanOptions
        {
            CropToPageSize = true,
            PageSize = new PageSize(8m, 10m, PageSizeUnit.Inch)
        };
        var result = _remotePostProcessor.PostProcess(image, options, new PostProcessingContext());
        ImageAsserts.Similar(ImageResources.patcht_cropped_bl, result!.Render().PerformTransform(new RotationTransform(90)));
    }

    [Fact]
    public void CropToPageSize_BothLandscape()
    {
        var image = LoadImage(ImageResources.patcht).PerformTransform(new RotationTransform(-90));
        var options = new ScanOptions
        {
            CropToPageSize = true,
            PageSize = new PageSize(10m, 8m, PageSizeUnit.Inch)
        };
        var result = _remotePostProcessor.PostProcess(image, options, new PostProcessingContext());
        ImageAsserts.Similar(ImageResources.patcht_cropped_bl, result!.Render().PerformTransform(new RotationTransform(90)));
    }

    [Fact]
    public void CropToPageSize_Scaled()
    {
        var image = LoadImage(ImageResources.patcht);
        var options = new ScanOptions
        {
            CropToPageSize = true,
            ScaleRatio = 2,
            PageSize = new PageSize(8m, 10m, PageSizeUnit.Inch)
        };
        var result = _remotePostProcessor.PostProcess(image, options, new PostProcessingContext());
        using var expected = LoadImage(ImageResources.patcht_cropped_br).PerformTransform(new ScaleTransform(0.5));
        using var rendered = result!.Render();
        ImageAsserts.Similar(expected, rendered, ImageAsserts.XPLAT_RMSE_THRESHOLD, ignoreResolution: true);
    }

    [Fact]
    public void StretchToPageSize_Scaled()
    {
        var image = LoadImage(ImageResources.patcht);
        int originalWidth = image.Width, originalHeight = image.Height;
        var options = new ScanOptions
        {
            StretchToPageSize = true,
            ScaleRatio = 2,
            PageSize = new PageSize(8m, 10m, PageSizeUnit.Inch)
        };
        var result = _remotePostProcessor.PostProcess(image, options, new PostProcessingContext());
        using var rendered = result!.Render();
        Assert.Equal(Math.Round(originalWidth / 2.0), rendered.Width);
        Assert.Equal(Math.Round(originalHeight / 2.0), rendered.Height);
    }

    // Only Linux can have zero resolution images
    [PlatformFact(include: PlatformFlags.GtkImage)]
    public void CropToPageSize_NoResolution()
    {
        var image = LoadImage(ImageResources.patcht);
        image.SetResolution(0, 0);
        var options = new ScanOptions
        {
            CropToPageSize = true,
            PageSize = new PageSize(8m, 10m, PageSizeUnit.Inch)
        };
        var result = _remotePostProcessor.PostProcess(image, options, new PostProcessingContext());
        ImageAsserts.Similar(ImageResources.patcht, result, ignoreResolution: true);
    }

    [Fact]
    public void PageSize()
    {
        var image = LoadImage(ImageResources.dog);
        var options = new ScanOptions
        {
            PageSize = NAPS2.Images.PageSize.A4
        };
        var result = _remotePostProcessor.PostProcess(image, options, new PostProcessingContext());
        Assert.NotNull(result?.Metadata.PageSize);
        Assert.Equal(210m, result.Metadata.PageSize.Width);
        Assert.Equal(297m, result.Metadata.PageSize.Height);
        Assert.Equal(PageSizeUnit.Millimetre, result.Metadata.PageSize.Unit);
    }

    [Fact]
    public void NoFlipDuplexed()
    {
        var image1 = LoadImage(ImageResources.dog);
        var image2 = LoadImage(ImageResources.dog);
        var options = new ScanOptions
        {
            PaperSource = PaperSource.Duplex
        };
        var result1 = _remotePostProcessor.PostProcess(image1, options, new PostProcessingContext
        {
            PageNumber = 1
        })!;
        var result2 = _remotePostProcessor.PostProcess(image2, options, new PostProcessingContext
        {
            PageNumber = 2
        })!;
        Assert.Equal(1, result1.PostProcessingData.PageNumber);
        Assert.Equal(PageSide.Front, result1.PostProcessingData.PageSide);
        ImageAsserts.Similar(ImageResources.dog, result1);
        Assert.Equal(2, result2.PostProcessingData.PageNumber);
        Assert.Equal(PageSide.Back, result2.PostProcessingData.PageSide);
        ImageAsserts.Similar(ImageResources.dog, result2);
    }

    [Fact]
    public void FlipDuplexed()
    {
        var image1 = LoadImage(ImageResources.dog);
        var image2 = LoadImage(ImageResources.dog);
        var options = new ScanOptions
        {
            PaperSource = PaperSource.Duplex,
            FlipDuplexedPages = true
        };
        var result1 = _remotePostProcessor.PostProcess(image1, options, new PostProcessingContext
        {
            PageNumber = 1
        })!;
        var result2 = _remotePostProcessor.PostProcess(image2, options, new PostProcessingContext
        {
            PageNumber = 2
        })!;
        Assert.Equal(1, result1.PostProcessingData.PageNumber);
        Assert.Equal(PageSide.Front, result1.PostProcessingData.PageSide);
        ImageAsserts.Similar(ImageResources.dog, result1);
        Assert.Equal(2, result2.PostProcessingData.PageNumber);
        Assert.Equal(PageSide.Back, result2.PostProcessingData.PageSide);
        ImageAsserts.Similar(ImageResources.dog, result2.WithTransform(new RotationTransform(180)));
    }

    [Theory]
    [InlineData(300, 0, false, CorrectionMode.None)]
    [InlineData(0, 90, true, CorrectionMode.Document)]
    public void OcrInputAndThumbnailMatchRenderedPage(int brightness, int rotateDegrees, bool autoCrop,
        CorrectionMode correctionMode)
    {
        var options = new ScanOptions
        {
            PaperSource = PaperSource.Duplex,
            FlipDuplexedPages = true,
            Brightness = brightness,
            RotateDegrees = rotateDegrees,
            AutoCrop = autoCrop,
            CorrectionMode = correctionMode,
            ThumbnailSize = 256,
            OcrParams = new OcrParams("eng")
        };
        var context = new PostProcessingContext { PageNumber = 2 };
        using var result = _remotePostProcessor.PostProcess(LoadImage(ImageResources.autocrop_doc), options, context)!;

        using var rendered = result.Render();
        Assert.NotNull(context.TempPath);
        using var ocrImage = ImageContext.Load(context.TempPath);
        ImageAsserts.Similar(rendered, ocrImage, ImageAsserts.XPLAT_RMSE_THRESHOLD);
        Assert.Equal(result.TransformState, result.PostProcessingData.ThumbnailTransformState);
        using var expectedThumbnail = rendered.Clone().PerformTransform(new ThumbnailTransform(256));
        ImageAsserts.Similar(expectedThumbnail, result.PostProcessingData.Thumbnail!, ImageAsserts.XPLAT_RMSE_THRESHOLD);
    }

    [Fact]
    public void NoOcrOrThumbnailByDefault()
    {
        var context = new PostProcessingContext();
        using var result = _remotePostProcessor.PostProcess(LoadImage(ImageResources.dog), new ScanOptions(), context)!;
        Assert.Null(context.TempPath);
        Assert.Null(result.PostProcessingData.Thumbnail);
        Assert.Null(result.PostProcessingData.ThumbnailTransformState);
    }

    [Theory]
    [ClassData(typeof(StorageAwareTestData))]
    public void TransformsDontChangeStoredImage(StorageConfig storageConfig)
    {
        storageConfig.Apply(this);
        var options = new ScanOptions
        {
            MaxQuality = true,
            RotateDegrees = 90,
            Brightness = 300,
            AutoCrop = true,
            CorrectionMode = CorrectionMode.Document,
            ThumbnailSize = 256,
            OcrParams = new OcrParams("eng")
        };
        using var result = _remotePostProcessor.PostProcess(LoadImage(ImageResources.autocrop_doc), options,
            new PostProcessingContext())!;
        Assert.NotEmpty(result.TransformState.Transforms);
        using var original = result.WithNoTransforms();
        ImageAsserts.Similar(ImageResources.autocrop_doc, original);
    }

    [Fact]
    public void AutoDeskewWithRotation()
    {
        using var deskewed = _remotePostProcessor.PostProcess(LoadImage(ImageResources.skewed),
            new ScanOptions { AutoDeskew = true }, new PostProcessingContext())!;
        using var result = _remotePostProcessor.PostProcess(LoadImage(ImageResources.skewed),
            new ScanOptions { AutoDeskew = true, RotateDegrees = 90 }, new PostProcessingContext())!;

        var deskewAngle = Assert.IsType<RotationTransform>(Assert.Single(deskewed.TransformState.Transforms)).Angle;
        Assert.NotEqual(0, deskewAngle);
        var totalAngle = result.TransformState.Transforms.Cast<RotationTransform>().Sum(x => x.Angle);
        Assert.Equal(RotationTransform.NormalizeAngle(90 + deskewAngle), RotationTransform.NormalizeAngle(totalAngle), 3);
    }

    [Fact]
    public void DetectBarcode()
    {
        var options = new ScanOptions
        {
            PaperSource = PaperSource.Duplex,
            FlipDuplexedPages = true,
            BarcodeDetectionOptions = new BarcodeDetectionOptions { DetectBarcodes = true, PatchTOnly = true }
        };
        using var result = _remotePostProcessor.PostProcess(LoadImage(ImageResources.patcht), options,
            new PostProcessingContext { PageNumber = 2 })!;
        Assert.True(result.PostProcessingData.Barcode.IsDetected);
        Assert.True(result.PostProcessingData.Barcode.IsPatchT);
    }

    [Fact]
    public void BarcodeDetectionDisabled()
    {
        using var result = _remotePostProcessor.PostProcess(LoadImage(ImageResources.patcht), new ScanOptions(),
            new PostProcessingContext())!;
        Assert.False(result.PostProcessingData.Barcode.IsDetectionAttempted);
    }

    [Theory]
    [InlineData(Driver.Escl, false, true)]
    [InlineData(Driver.Wia, false, false)]
    [InlineData(Driver.Twain, false, false)]
    [InlineData(Driver.Wia, true, true)]
    public void BrightnessContrastAfterScan(Driver driver, bool afterScan, bool expectTransforms)
    {
        var options = new ScanOptions
        {
            Driver = driver,
            Brightness = 300,
            Contrast = 200,
            BrightnessContrastAfterScan = afterScan
        };
        using var result = _remotePostProcessor.PostProcess(LoadImage(ImageResources.dog), options,
            new PostProcessingContext())!;
        if (expectTransforms)
        {
            Assert.Equal([new BrightnessTransform(300), new TrueContrastTransform(200)], result.TransformState.Transforms);
        }
        else
        {
            Assert.Empty(result.TransformState.Transforms);
        }
    }
}