using NAPS2.Scan;
using NAPS2.Scan.Internal;
using NAPS2.Sdk.Tests.Asserts;
using Xunit;

namespace NAPS2.Sdk.Tests.Scan;

public class RemotePostProcessorTests : ContextualTests
{
    // TODO: Add more tests

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
}