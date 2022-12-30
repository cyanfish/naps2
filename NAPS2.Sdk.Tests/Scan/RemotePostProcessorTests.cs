using NAPS2.Scan;
using NAPS2.Scan.Internal;
using Xunit;

namespace NAPS2.Sdk.Tests.Scan;

public class RemotePostProcessorTests : ContextualTests
{
    // TODO: Add more tests

    private RemotePostProcessor _remotePostProcessor;

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
            ExcludeBlankPages = true,
            BlankPageWhiteThreshold = 70,
            BlankPageCoverageThreshold = 15
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
            ExcludeBlankPages = true,
            BlankPageWhiteThreshold = 70,
            BlankPageCoverageThreshold = 15
        };
        var result = _remotePostProcessor.PostProcess(image, options, new PostProcessingContext());
        Assert.NotNull(result);
    }
}