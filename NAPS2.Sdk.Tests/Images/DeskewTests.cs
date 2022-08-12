using NAPS2.Sdk.Tests.Asserts;
using Xunit;

namespace NAPS2.Sdk.Tests.Images;

public class DeskewTests : ContextualTests
{
    [Fact]
    public void Deskew()
    {
        var image = LoadImage(ImageResources.skewed);
        var skewAngle = Deskewer.GetSkewAngle(image);
        Assert.InRange(skewAngle, 15.5, 16.5);
    }

    [Fact]
    public void DeskewBlackAndWhite()
    {
        var image = LoadImage(ImageResources.skewed_bw);
        var skewAngle = Deskewer.GetSkewAngle(image);
        Assert.InRange(skewAngle, 15.5, 16.5);
    }

    [Fact]
    public void DeskewTransform()
    {
        var image = LoadImage(ImageResources.skewed);
        var transform = Deskewer.GetDeskewTransform(image);
        var deskewedImage = ImageContext.PerformTransform(image, transform);
        ImageAsserts.Similar(ImageResources.deskewed, deskewedImage);
    }

    [Fact]
    public void NoSkewAngle()
    {
        // The cat picture doesn't have consistent lines, so deskewing should be a no-op
        var image = LoadImage(ImageResources.stock_cat);
        var skewAngle = Deskewer.GetSkewAngle(image);
        Assert.Equal(0, skewAngle);
    }
}