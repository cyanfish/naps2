using NAPS2.Images.Bitwise;
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

        var image = LoadImage(ImageResources.color_image);
        var processedImage = ScanningContext.CreateProcessedImage(image);

        var rendered = ImageContext.Render(processedImage);

        ImageAsserts.Similar(ImageResources.color_image, rendered);
    }
    
    [Theory]
    [ClassData(typeof(StorageAwareTestData))]
    public void RenderGray(StorageConfig config)
    {
        config.Apply(this);

        // TODO: Have an actual gray image to load
        var image = LoadImage(ImageResources.color_image);
        var grayImage = ImageContext.Create(image.Width, image.Height, ImagePixelFormat.Gray8);
        new CopyBitwiseImageOp().Perform(image, grayImage);
        var processedImage = ScanningContext.CreateProcessedImage(grayImage);

        var rendered = ImageContext.Render(processedImage);

        ImageAsserts.Similar(grayImage, rendered);
    }

    [Theory]
    [ClassData(typeof(StorageAwareTestData))]
    public void RenderBlackAndWhite(StorageConfig config)
    {
        config.Apply(this);

        var image = LoadImage(ImageResources.color_image_bw);
        image = ImageContext.PerformTransform(image, new BlackWhiteTransform());
        var processedImage = ScanningContext.CreateProcessedImage(image);

        var rendered = ImageContext.Render(processedImage);

        ImageAsserts.Similar(ImageResources.color_image_bw, rendered);
    }
}