using NAPS2.Images.Gdi;
using NAPS2.Sdk.Tests.Asserts;
using Xunit;

namespace NAPS2.Sdk.Tests.Images;

public class DeskewTests : ContextualTests
{
    [Fact]
    public void Deskew()
    {
        var image = new GdiImage(DeskewTestsData.skewed);
        Assert.Equal(ImagePixelFormat.RGB24, image.PixelFormat);
        var skewAngle = Deskewer.GetSkewAngle(image);
        Assert.InRange(skewAngle, 15.5, 16.5);
    }

    [Fact]
    public void DeskewBlackAndWhite()
    {
        var image = new GdiImage(DeskewTestsData.skewed_bw);
        Assert.Equal(ImagePixelFormat.BW1, image.PixelFormat);
        var skewAngle = Deskewer.GetSkewAngle(image);
        Assert.InRange(skewAngle, 15.5, 16.5);
    }
        
    [Fact]
    public void DeskewTransform()
    {
        var image = new GdiImage(DeskewTestsData.skewed);
        Assert.Equal(ImagePixelFormat.RGB24, image.PixelFormat);
        var transform = Deskewer.GetDeskewTransform(image);
        var deskewedImage = ImageContext.PerformTransform(image, transform);
        ImageAsserts.Similar(DeskewTestsData.deskewed, deskewedImage);
    }
        
    [Fact]
    public void NoSkewAngle()
    {
        // The cat picture doesn't have consistent lines, so deskewing should be a no-op
        var image = new GdiImage(DeskewTestsData.stock_cat);
        Assert.Equal(ImagePixelFormat.RGB24, image.PixelFormat);
        var skewAngle = Deskewer.GetSkewAngle(image);
        Assert.Equal(0, skewAngle);
    }
}