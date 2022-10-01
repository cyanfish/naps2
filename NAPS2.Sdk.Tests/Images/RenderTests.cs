using NAPS2.Sdk.Tests.Asserts;
using Xunit;

namespace NAPS2.Sdk.Tests.Images;

public class RenderTests : ContextualTests
{
    [Theory]
    [ClassData(typeof(StorageAwareTestData))]
    public void RenderColor(StorageConfig config)
    {
        config.Apply(this);

        var image = LoadImage(ImageResources.dog);
        var processedImage = ScanningContext.CreateProcessedImage(image);

        var rendered = processedImage.Render();

        ImageAsserts.Similar(ImageResources.dog, rendered);
    }
    
    [Theory]
    [ClassData(typeof(StorageAwareTestData))]
    public void RenderGray(StorageConfig config)
    {
        config.Apply(this);

        // TODO: Have an actual gray image to load
        var image = LoadImage(ImageResources.dog);
        var grayImage = image.CopyWithPixelFormat(ImagePixelFormat.Gray8);
        var processedImage = ScanningContext.CreateProcessedImage(grayImage);

        var rendered = processedImage.Render();

        ImageAsserts.Similar(grayImage, rendered);
    }

    [Theory]
    [ClassData(typeof(StorageAwareTestData))]
    public void RenderBlackAndWhite(StorageConfig config)
    {
        config.Apply(this);

        var image = LoadImage(ImageResources.dog_bw);
        image = image.PerformTransform(new BlackWhiteTransform());
        var processedImage = ScanningContext.CreateProcessedImage(image);

        var rendered = processedImage.Render();

        ImageAsserts.Similar(ImageResources.dog_bw, rendered);
    }
}