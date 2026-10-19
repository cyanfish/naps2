#if WINDOWS
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using NAPS2.Images.Wpf;
using Xunit;

namespace NAPS2.Sdk.Tests.Images;

[System.Runtime.Versioning.SupportedOSPlatform("windows7.0")]
public class WpfImageTests
{
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
