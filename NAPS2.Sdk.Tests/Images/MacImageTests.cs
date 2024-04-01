#if MAC
using MonoMac.AppKit;
using MonoMac.CoreGraphics;
using NAPS2.Images.Mac;
using NAPS2.Sdk.Tests.Asserts;
using Xunit;

namespace NAPS2.Sdk.Tests.Images;

public class MacImageTests : ContextualTests
{
    [Theory]
    [InlineData(ImagePixelFormat.ARGB32)]
    [InlineData(ImagePixelFormat.RGB24)]
    [InlineData(ImagePixelFormat.Gray8)]
    [InlineData(ImagePixelFormat.BW1)]
    public void UsesCorrectPixelFormat(ImagePixelFormat pixelFormat)
    {
        var nsImage = new NSImage();
        var rep = MacBitmapHelper.CreateRep(100, 100, pixelFormat);
        nsImage.AddRepresentation(rep);
        var image = new MacImage(nsImage);
        Assert.Equal(pixelFormat, image.PixelFormat);
        Assert.Equal(rep.Handle, image.Rep.Handle);
    }

    [Fact]
    public void ThrowsOnNoReps()
    {
        var nsImage = new NSImage();
        Assert.Throws<ArgumentException>(() => new MacImage(nsImage));
    }

    [Fact]
    public void ThrowsOnMultipleReps()
    {
        var nsImage = new NSImage();
        nsImage.AddRepresentation(MacBitmapHelper.CreateRep(100, 100, ImagePixelFormat.ARGB32));
        nsImage.AddRepresentation(MacBitmapHelper.CreateRep(100, 100, ImagePixelFormat.ARGB32));
        Assert.Throws<ArgumentException>(() => new MacImage(nsImage));
    }

    [Theory]
    [InlineData(ImagePixelFormat.ARGB32)]
    [InlineData(ImagePixelFormat.RGB24)]
    [InlineData(ImagePixelFormat.Gray8)]
    public void ConvertsUnexpectedColorSpace(ImagePixelFormat pixelFormat)
    {
        var nsImage = new NSImage();
        var rep = MacBitmapHelper.CreateRep(100, 100, pixelFormat);
        var colorSpace = pixelFormat is ImagePixelFormat.ARGB32 or ImagePixelFormat.RGB24
            ? NSColorSpace.GenericRGBColorSpace
            : NSColorSpace.GenericGrayColorSpace;
        rep = rep.ConvertingToColorSpace(colorSpace, NSColorRenderingIntent.Default);
        nsImage.AddRepresentation(rep);
        var image = new MacImage(nsImage);
        Assert.NotEqual(rep.Handle, image.Rep.Handle);
        Assert.Equal(pixelFormat, image.PixelFormat);
    }

    [Fact]
    public void ConvertsBlackColorSpace()
    {
        var nsImage = new NSImage();
        var rep = new NSBitmapImageRep(IntPtr.Zero, 100, 100, 1, 1, false, false, NSColorSpace.DeviceBlack, 13, 1);
        nsImage.AddRepresentation(rep);
        var image = new MacImage(nsImage);
        Assert.NotEqual(rep.Handle, image.Rep.Handle);
        Assert.Equal(ImagePixelFormat.Gray8, image.PixelFormat);
    }

    [Fact]
    public void ConvertsUnsupportedPixelFormat()
    {
        var referenceImage = (MacImage) LoadImage(ImageResources.dog);
        var nsImage = new NSImage();
        var rep = Create64BitRepFromImage(referenceImage);
        nsImage.AddRepresentation(rep);
        var image = new MacImage(nsImage);
        Assert.NotEqual(rep.Handle, image.Rep.Handle);
        ImageAsserts.Similar(referenceImage, image);
    }

    private static NSBitmapImageRep Create64BitRepFromImage(MacImage testImage)
    {
        var w = testImage.Width;
        var h = testImage.Height;
        var rep = new NSBitmapImageRep(IntPtr.Zero, w, h, 16, 3, false, false, NSColorSpace.DeviceRGB, w * 8, 64);
        using var ctx = MacBitmapHelper.CreateContext(rep, false, false);
        ctx.DrawImage(new CGRect(0, 0, w, h), testImage.Rep.CGImage);
        return rep;
    }
}
#endif