using System.Drawing;
using System.Drawing.Imaging;
using NAPS2.Images.Gdi;
using NAPS2.Sdk.Tests.Asserts;
using Xunit;

namespace NAPS2.Sdk.Tests.Images;

public class GdiImageTests
{
    [PlatformFact(include: PlatformFlags.Windows)]
    public void ImageContextCreateDoesntFixFormat()
    {
        var ctx = new GdiImageContext();
        var imageArgb32 = (GdiImage) ctx.Create(1, 1, ImagePixelFormat.ARGB32);
        var imageRgb24 = (GdiImage) ctx.Create(1, 1, ImagePixelFormat.RGB24);
        var imageGray8 = (GdiImage) ctx.Create(1, 1, ImagePixelFormat.Gray8);
        var imageBw1 = (GdiImage) ctx.Create(1, 1, ImagePixelFormat.BW1);

        Assert.False(imageArgb32.FixedPixelFormat);
        Assert.False(imageRgb24.FixedPixelFormat);
        Assert.False(imageGray8.FixedPixelFormat);
        Assert.False(imageBw1.FixedPixelFormat);
    }

    [PlatformFact(include: PlatformFlags.Windows)]
    public void LoadInvertedPaletteBlackAndWhiteImage()
    {
        var bitmap = new Bitmap(new MemoryStream(ImageResources.color_image_bw_invertpal));

        var image = new GdiImage(bitmap);
        Assert.True(image.FixedPixelFormat);
        Assert.Equal(ImagePixelFormat.BW1, image.PixelFormat);
        Assert.Equal(Color.Black.ToArgb(), image.Bitmap.Palette.Entries[0].ToArgb());
        Assert.Equal(Color.White.ToArgb(), image.Bitmap.Palette.Entries[1].ToArgb());
        ImageAsserts.Similar(ImageResources.color_image_bw, image, ImageAsserts.NULL_RMSE_THRESHOLD);
    }

    [PlatformFact(include: PlatformFlags.Windows)]
    public void LoadNonGrayscale8BitImage()
    {
        var workingImage = new GdiImageContext().Create(1, 1, ImagePixelFormat.Gray8);
        var bitmap = ((GdiImage) workingImage).Bitmap;
        var p = bitmap.Palette;
        p.Entries[128] = Color.Blue;
        bitmap.Palette = p;

        var image = new GdiImage(bitmap);
        Assert.True(image.FixedPixelFormat);
        Assert.Equal(ImagePixelFormat.RGB24, image.PixelFormat);
    }

    [PlatformFact(include: PlatformFlags.Windows)]
    public void Load48BitImage()
    {
        var bitmap = new Bitmap(1, 1, PixelFormat.Format48bppRgb);

        var image = new GdiImage(bitmap);
        Assert.True(image.FixedPixelFormat);
        Assert.Equal(ImagePixelFormat.RGB24, image.PixelFormat);
    }

    [PlatformFact(include: PlatformFlags.Windows)]
    public void Load64BitImage()
    {
        var bitmap = new Bitmap(1, 1, PixelFormat.Format64bppArgb);

        var image = new GdiImage(bitmap);
        Assert.True(image.FixedPixelFormat);
        Assert.Equal(ImagePixelFormat.ARGB32, image.PixelFormat);
    }
}