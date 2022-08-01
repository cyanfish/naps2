using System.Collections;
using System.Drawing;
using NAPS2.Images.Gdi;
using Xunit;
using Xunit.Sdk;

namespace NAPS2.Sdk.Tests.Asserts;

public static class ImageAsserts
{
    // JPEG artifacts seem to consistently create a RMSE of about 2.5.
    // TODO: Use PNG or some other way to do a precise comparison.
    public const double GENERAL_RMSE_THRESHOLD = 3.5;

    public const double NULL_RMSE_THRESHOLD = 0.6;

    private const double RESOLUTION_THRESHOLD = 0.1;

    private const double DIMENSIONS_THRESHOLD = 0.05;

    public static void Similar(Bitmap first, ProcessedImage second, double rmseThreshold = GENERAL_RMSE_THRESHOLD,
        bool ignoreFormat = false)
    {
        using var rendered = new GdiImageContext().Render(second);
        Similar(new GdiImage(first), rendered, rmseThreshold, ignoreFormat);
    }

    public static void Similar(Bitmap first, IMemoryImage second, double rmseThreshold = GENERAL_RMSE_THRESHOLD,
        bool ignoreFormat = false)
    {
        Similar(new GdiImage(first), second, rmseThreshold, ignoreFormat);
    }

    public static void Similar(IMemoryImage first, IMemoryImage second,
        double rmseThreshold = GENERAL_RMSE_THRESHOLD, bool ignoreFormat = false)
    {
        Similar(first, second, rmseThreshold, ignoreFormat, true);
    }

    public static void NotSimilar(Bitmap first, ProcessedImage second, double rmseThreshold = GENERAL_RMSE_THRESHOLD,
        bool ignoreFormat = false)
    {
        using var rendered = new GdiImageContext().Render(second);
        NotSimilar(new GdiImage(first), rendered, rmseThreshold, ignoreFormat);
    }

    public static void NotSimilar(Bitmap first, IMemoryImage second, double rmseThreshold = GENERAL_RMSE_THRESHOLD,
        bool ignoreFormat = false)
    {
        NotSimilar(new GdiImage(first), second, rmseThreshold, ignoreFormat);
    }

    public static void NotSimilar(IMemoryImage first, IMemoryImage second,
        double rmseThreshold = GENERAL_RMSE_THRESHOLD, bool ignoreFormat = false)
    {
        Similar(first, second, rmseThreshold, ignoreFormat, false);
    }

    private static unsafe void Similar(IMemoryImage first, IMemoryImage second,
        double rmseThreshold, bool ignoreFormat, bool isSimilar)
    {
        if (first.PixelFormat != ImagePixelFormat.BW1 && first.PixelFormat != ImagePixelFormat.RGB24 &&
            first.PixelFormat != ImagePixelFormat.ARGB32)
        {
            throw new InvalidOperationException("Unsupported pixel format");
        }

        Assert.Equal(first.Width, second.Width);
        Assert.Equal(first.Height, second.Height);
        if (!ignoreFormat)
        {
            Assert.Equal(first.PixelFormat, second.PixelFormat);
        }
        Assert.InRange(second.HorizontalResolution,
            first.HorizontalResolution - RESOLUTION_THRESHOLD,
            first.HorizontalResolution + RESOLUTION_THRESHOLD);
        Assert.InRange(second.VerticalResolution,
            first.VerticalResolution - RESOLUTION_THRESHOLD,
            first.VerticalResolution + RESOLUTION_THRESHOLD);

        var imageContext = new GdiImageContext();
        first = imageContext.PerformTransform(first, new ColorBitDepthTransform());
        second = imageContext.PerformTransform(second, new ColorBitDepthTransform());

        using var lock1 = first.Lock(LockMode.ReadOnly, out var scan01, out var stride1);
        using var lock2 = second.Lock(LockMode.ReadOnly, out var scan02, out var stride2);
        int width = first.Width;
        int height = first.Height;
        int bytesPerPixel = first.PixelFormat == ImagePixelFormat.ARGB32 ? 4 : 3;
        long total = 0;
        long div = width * height * 3;
        byte* data1 = (byte*) scan01;
        byte* data2 = (byte*) scan02;
        for (int y = 0; y < height; y++)
        {
            byte* row1 = data1 + stride1 * y;
            byte* row2 = data2 + stride2 * y;
            for (int x = 0; x < width; x++)
            {
                byte* pixel1 = row1 + x * bytesPerPixel;
                byte* pixel2 = row2 + x * bytesPerPixel;

                byte r1 = *pixel1;
                byte g1 = *(pixel1 + 1);
                byte b1 = *(pixel1 + 2);

                byte r2 = *pixel2;
                byte g2 = *(pixel2 + 1);
                byte b2 = *(pixel2 + 2);

                total += (r1 - r2) * (r1 - r2) + (g1 - g2) * (g1 - g2) + (b1 - b2) * (b1 - b2);

                if (bytesPerPixel == 4)
                {
                    byte a1 = *(pixel1 + 3);
                    byte a2 = *(pixel2 + 3);
                    total += (a1 - a2) * (a1 - a2);
                }
            }
        }

        double rmse = Math.Sqrt(total / (double) div);
        if (isSimilar)
        {
            Assert.True(rmse <= rmseThreshold, $"RMSE was {rmse}, expected <= {rmseThreshold}");
        }
        else
        {
            Assert.True(rmse > rmseThreshold, $"RMSE was {rmse}, expected > {rmseThreshold}");
        }
    }

    public static void PixelColors(IMemoryImage image, PixelColorData colorData)
    {
        var bitmap = ((GdiImage) image).Bitmap;
        foreach (var data in colorData)
        {
            var (x, y) = data.Item1;
            var (r, g, b) = data.Item2;
            var color = bitmap.GetPixel(x, y);
            var expected = Color.FromArgb(r, g, b);
            if (color != expected)
            {
                throw new AssertActualExpectedException(expected, color, $"Mismatched color at ({x}, {y})");
            }
        }
    }

    public class PixelColorData : IEnumerable<((int x, int y), (int r, int g, int b))>
    {
        private readonly List<((int x, int y), (int r, int g, int b))> _colors = new();

        public void Add((int x, int y) pos, (int r, int g, int b) color)
        {
            _colors.Add((pos, color));
        }

        public void Add((int x, int y) pos, Color color)
        {
            _colors.Add((pos, (color.R, color.G, color.B)));
        }

        public IEnumerator<((int x, int y), (int r, int g, int b))> GetEnumerator()
        {
            return _colors.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public static void Inches(string path, double expectedWidth, int expectedHeight)
    {
        var image = new GdiImageContext().Load(path);
        var actualWidth = image.Width / image.HorizontalResolution;
        var actualHeight = image.Height / image.VerticalResolution;
        Assert.InRange(actualWidth, expectedWidth - DIMENSIONS_THRESHOLD, expectedWidth + DIMENSIONS_THRESHOLD);
        Assert.InRange(actualHeight, expectedHeight - DIMENSIONS_THRESHOLD, expectedHeight + DIMENSIONS_THRESHOLD);
    }
}