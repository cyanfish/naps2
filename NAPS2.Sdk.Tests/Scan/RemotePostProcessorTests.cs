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
        ImageAsserts.Similar(ImageResources.deskewed, result, ImageAsserts.XPLAT_RMSE_THRESHOLD);
    }
}