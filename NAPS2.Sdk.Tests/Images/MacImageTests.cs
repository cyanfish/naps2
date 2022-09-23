#if MAC
using MonoMac.AppKit;
using NAPS2.Images.Mac;
using Xunit;

namespace NAPS2.Sdk.Tests.Images;

public class MacImageTests
{
    private readonly ImageContext _imageContext = new MacImageContext();

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
        var image = new MacImage(_imageContext, nsImage);
        Assert.Equal(pixelFormat, image.PixelFormat);
        Assert.Equal(rep.Handle, image._imageRep.Handle);
    }

    [Fact]
    public void ThrowsOnNoReps()
    {
        var nsImage = new NSImage();
        Assert.Throws<ArgumentException>(() => new MacImage(_imageContext, nsImage));
    }

    [Fact]
    public void ThrowsOnMultipleReps()
    {
        var nsImage = new NSImage();
        nsImage.AddRepresentation(MacBitmapHelper.CreateRep(100, 100, ImagePixelFormat.ARGB32));
        nsImage.AddRepresentation(MacBitmapHelper.CreateRep(100, 100, ImagePixelFormat.ARGB32));
        Assert.Throws<ArgumentException>(() => new MacImage(_imageContext, nsImage));
    }

    [Theory]
    [InlineData(ImagePixelFormat.ARGB32)]
    [InlineData(ImagePixelFormat.RGB24)]
    [InlineData(ImagePixelFormat.Gray8)]
    [InlineData(ImagePixelFormat.BW1)]
    public void ConvertsUnexpectedColorSpace(ImagePixelFormat pixelFormat)
    {
        var nsImage = new NSImage();
        var rep = MacBitmapHelper.CreateRep(100, 100, pixelFormat);
        rep = rep.ConvertingToColorSpace(NSColorSpace.GenericRGBColorSpace, NSColorRenderingIntent.Default);
        nsImage.AddRepresentation(rep);
        var image = new MacImage(_imageContext, nsImage);
        Assert.NotEqual(rep.Handle, image._imageRep.Handle);
        // TODO: Do we want to check the pixel format is the same as the original?
    }

    [Theory]
    [InlineData(ImagePixelFormat.ARGB32)]
    [InlineData(ImagePixelFormat.RGB24)]
    public void DoesntConvertSrgbColorSpace(ImagePixelFormat pixelFormat)
    {
        var nsImage = new NSImage();
        var rep = MacBitmapHelper.CreateRep(100, 100, pixelFormat);
        rep = rep.ConvertingToColorSpace(NSColorSpace.SRGBColorSpace, NSColorRenderingIntent.Default);
        nsImage.AddRepresentation(rep);
        var image = new MacImage(_imageContext, nsImage);
        Assert.Equal(pixelFormat, image.PixelFormat);
        Assert.Equal(rep.Handle, image._imageRep.Handle);
    }

    [Fact]
    public void ConvertsUnsupportedPixelFormat()
    {
        var nsImage = new NSImage();
        var rep = new NSBitmapImageRep(IntPtr.Zero, 100, 100, 16, 3, false, false, NSColorSpace.DeviceRGB, 600, 48);
        nsImage.AddRepresentation(rep);
        var image = new MacImage(_imageContext, nsImage);
        Assert.NotEqual(rep.Handle, image._imageRep.Handle);
    }
}
#endif