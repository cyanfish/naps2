#if WINDOWS
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using NAPS2.Images.Wpf;
using NAPS2.Sdk.Tests.Asserts;
using Xunit;

namespace NAPS2.Sdk.Tests.Images;

[System.Runtime.Versioning.SupportedOSPlatform("windows7.0")]
public class WpfImageTests
{
    [Fact]
    public void ImageContextCreateUsesRequestedFormat()
    {
        var ctx = new WpfImageContext();
        using var imageArgb32 = ctx.Create(1, 1, ImagePixelFormat.ARGB32);
        using var imageRgb24 = ctx.Create(1, 1, ImagePixelFormat.RGB24);
        using var imageGray8 = ctx.Create(1, 1, ImagePixelFormat.Gray8);
        using var imageBw1 = ctx.Create(1, 1, ImagePixelFormat.BW1);

        Assert.Equal(ImagePixelFormat.ARGB32, imageArgb32.PixelFormat);
        Assert.Equal(ImagePixelFormat.RGB24, imageRgb24.PixelFormat);
        Assert.Equal(ImagePixelFormat.Gray8, imageGray8.PixelFormat);
        Assert.Equal(ImagePixelFormat.BW1, imageBw1.PixelFormat);
    }

    [Fact]
    public void ImageContextCreateLargerThan2GB()
    {
        // ~3GB, which would otherwise crash when using image operations
        // TODO: Maybe support images over 2GB?
        Assert.Throws<NotSupportedException>(() =>
            new WpfImageContext().Create(32000, 33000, ImagePixelFormat.RGB24));
    }

    [Fact]
    public void LoadInvertedPaletteBlackAndWhiteImage()
    {
        var decoder = BitmapDecoder.Create(new MemoryStream(ImageResources.dog_bw_invertpal),
            BitmapCreateOptions.None, BitmapCacheOption.OnLoad);

        using var image = new WpfImage(decoder.Frames[0]);
        Assert.Equal(ImagePixelFormat.BW1, image.PixelFormat);
        ImageAsserts.Similar(ImageResources.dog_bw, image, ImageAsserts.NULL_RMSE_THRESHOLD);
    }

    [Fact]
    public void LoadNonGrayscale8BitImage()
    {
        var colors = Enumerable.Range(0, 256).Select(i => Color.FromRgb((byte) i, (byte) i, (byte) i)).ToList();
        colors[128] = Colors.Blue;
        var bitmap = BitmapSource.Create(1, 1, 96, 96, PixelFormats.Indexed8, new BitmapPalette(colors),
            new byte[4], 4);

        using var image = new WpfImage(bitmap);
        Assert.Equal(ImagePixelFormat.RGB24, image.PixelFormat);
    }

    [Fact]
    public void Load48BitImage()
    {
        using var image = new WpfImage(CreateBlank(PixelFormats.Rgb48));

        Assert.Equal(ImagePixelFormat.RGB24, image.PixelFormat);
    }

    [Fact]
    public void Load64BitImage()
    {
        using var image = new WpfImage(CreateBlank(PixelFormats.Rgba64));

        Assert.Equal(ImagePixelFormat.ARGB32, image.PixelFormat);
    }

    [Fact]
    public void LoadBgr32Image()
    {
        // The 4th byte is padding (0 here) rather than alpha, so the image should be stored as opaque
        using var image = new WpfImage(CreateBlank(PixelFormats.Bgr32));

        Assert.Equal(ImagePixelFormat.RGB24, image.PixelFormat);
        AssertPixels(image, Color.FromRgb(0, 0, 0));
    }

    [Fact]
    public void LoadCmykImage()
    {
        // 4 channels but no alpha
        using var image = new WpfImage(CreateBlank(PixelFormats.Cmyk32));

        Assert.Equal(ImagePixelFormat.RGB24, image.PixelFormat);
    }

    [Fact]
    public void Load16BitGrayImage()
    {
        using var image = new WpfImage(CreateBlank(PixelFormats.Gray16));

        Assert.Equal(ImagePixelFormat.Gray8, image.PixelFormat);
    }

    [Fact]
    public void LoadGrayPalette()
    {
        using var image = new WpfImage(CreateIndexed8(Color.FromRgb(0, 0, 0), Color.FromRgb(128, 128, 128)));

        Assert.Equal(ImagePixelFormat.Gray8, image.PixelFormat);
        AssertPixels(image, Color.FromRgb(0, 0, 0), Color.FromRgb(128, 128, 128));
    }

    [Fact]
    public void LoadColorPalette()
    {
        using var image = new WpfImage(CreateIndexed8(Color.FromRgb(255, 0, 0), Color.FromRgb(0, 0, 255)));

        Assert.Equal(ImagePixelFormat.RGB24, image.PixelFormat);
        AssertPixels(image, Color.FromRgb(255, 0, 0), Color.FromRgb(0, 0, 255));
    }

    [Fact]
    public void LoadPaletteWithTransparency()
    {
        using var image = new WpfImage(CreateIndexed8(Color.FromRgb(255, 0, 0), Color.FromArgb(0, 0, 0, 255)));

        Assert.Equal(ImagePixelFormat.ARGB32, image.PixelFormat);
        AssertPixels(image, Color.FromRgb(255, 0, 0), Color.FromArgb(0, 0, 0, 255));
    }

    /// <summary>
    /// Creates a 1x1 bitmap with zeroed pixel data.
    /// </summary>
    private static BitmapSource CreateBlank(PixelFormat format)
    {
        int stride = (format.BitsPerPixel + 31) / 32 * 4;
        return BitmapSource.Create(1, 1, 96, 96, format, null, new byte[stride], stride);
    }

    /// <summary>
    /// Creates a 2x1 Indexed8 bitmap with one pixel for each palette color.
    /// </summary>
    private static BitmapSource CreateIndexed8(Color first, Color second)
    {
        var palette = new BitmapPalette(new List<Color> { first, second });
        return BitmapSource.Create(2, 1, 96, 96, PixelFormats.Indexed8, palette, new byte[] { 0, 1, 0, 0 }, 4);
    }

    private static void AssertPixels(IMemoryImage image, params Color[] expected)
    {
        using var argb = image.CopyWithPixelFormat(ImagePixelFormat.ARGB32);
        using var lockState = argb.Lock(LockMode.ReadOnly, out var data);
        for (int x = 0; x < expected.Length; x++)
        {
            var pixel = data.safePtr + x * data.bytesPerPixel;
            var actual = Color.FromArgb(Marshal.ReadByte(pixel, data.aOff), Marshal.ReadByte(pixel, data.rOff),
                Marshal.ReadByte(pixel, data.gOff), Marshal.ReadByte(pixel, data.bOff));
            Assert.Equal(expected[x], actual);
        }
    }
}
#endif
