using System.Collections;
using NAPS2.Images.Bitwise;
using Xunit;
using Xunit.Sdk;

namespace NAPS2.Sdk.Tests.Asserts;

public static class ImageAsserts
{
    // For slight cross-platform differences
    public const double XPLAT_RMSE_THRESHOLD = 6.5;
    
    // JPEG artifacts seem to consistently create a RMSE of about 2.5.
    public const double GENERAL_RMSE_THRESHOLD = 3.5;

    public const double NULL_RMSE_THRESHOLD = 0.6;

    // TODO: See if we can narrow this down
    private const double RESOLUTION_THRESHOLD = 0.05;

    private const double DIMENSIONS_THRESHOLD = 0.05;

    public static void Similar(byte[] first, ProcessedImage second, double rmseThreshold = GENERAL_RMSE_THRESHOLD, bool ignoreResolution = false)
    {
        using var rendered = second.Render();
        Similar(TestImageContextFactory.Get().Load(first), rendered, rmseThreshold, ignoreResolution);
    }

    public static void Similar(byte[] first, string secondPath, double rmseThreshold = GENERAL_RMSE_THRESHOLD, bool ignoreResolution = false)
    {
        using var second = TestImageContextFactory.Get().Load(secondPath);
        Similar(TestImageContextFactory.Get().Load(first), second, rmseThreshold, ignoreResolution);
    }

    public static void Similar(byte[] first, IMemoryImage second, double rmseThreshold = GENERAL_RMSE_THRESHOLD, bool ignoreResolution = false)
    {
        Similar(TestImageContextFactory.Get().Load(first), second, rmseThreshold, ignoreResolution);
    }

    public static void Similar(IMemoryImage first, IMemoryImage second,
        double rmseThreshold = GENERAL_RMSE_THRESHOLD, bool ignoreResolution = false)
    {
        Similar(first, second, rmseThreshold, ignoreResolution, true);
    }

    public static void NotSimilar(byte[] first, ProcessedImage second, double rmseThreshold = GENERAL_RMSE_THRESHOLD, bool ignoreResolution = false)
    {
        using var rendered = second.Render();
        NotSimilar(TestImageContextFactory.Get().Load(first), rendered, rmseThreshold, ignoreResolution);
    }

    public static void NotSimilar(byte[] first, IMemoryImage second, double rmseThreshold = GENERAL_RMSE_THRESHOLD, bool ignoreResolution = false)
    {
        NotSimilar(TestImageContextFactory.Get().Load(first), second, rmseThreshold, ignoreResolution);
    }

    public static void NotSimilar(IMemoryImage first, IMemoryImage second,
        double rmseThreshold = GENERAL_RMSE_THRESHOLD, bool ignoreResolution = false)
    {
        Similar(first, second, rmseThreshold, ignoreResolution, false);
    }

    private static unsafe void Similar(IMemoryImage first, IMemoryImage second,
        double rmseThreshold, bool ignoreResolution, bool isSimilar)
    {
        if (first.PixelFormat == ImagePixelFormat.Unsupported || second.PixelFormat == ImagePixelFormat.Unsupported)
        {
            throw new InvalidOperationException($"Unsupported pixel formats {first.PixelFormat} {second.PixelFormat}");
        }

        Assert.Equal(first.Width, second.Width);
        Assert.Equal(first.Height, second.Height);
        if (!ignoreResolution)
        {
            Assert.InRange(second.HorizontalResolution,
                first.HorizontalResolution - RESOLUTION_THRESHOLD,
                first.HorizontalResolution + RESOLUTION_THRESHOLD);
            Assert.InRange(second.VerticalResolution,
                first.VerticalResolution - RESOLUTION_THRESHOLD,
                first.VerticalResolution + RESOLUTION_THRESHOLD);
        }

        if (first.PixelFormat is ImagePixelFormat.BW1 or ImagePixelFormat.Gray8)
        {
            first = first.CopyWithPixelFormat(ImagePixelFormat.RGB24);
        }
        if (second.PixelFormat is ImagePixelFormat.BW1 or ImagePixelFormat.Gray8)
        {
            second = second.CopyWithPixelFormat(ImagePixelFormat.RGB24);
        }

        var op = new RmseBitwiseImageOp();
        op.Perform(first, second);
        var rmse = op.Rmse;
        if (isSimilar)
        {
            Assert.True(rmse <= rmseThreshold, $"RMSE was {rmse}, expected <= {rmseThreshold}");
        }
        else
        {
            Assert.True(rmse > rmseThreshold, $"RMSE was {rmse}, expected > {rmseThreshold}");
        }
    }

    public static unsafe void PixelColors(IMemoryImage image, PixelColorData colorData)
    {
        using var pixelReader = new RgbPixelReader(image);
        foreach (var data in colorData)
        {
            var (x, y) = data.Item1;
            var (r, g, b) = data.Item2;
            // TODO: 1bpp?
            var color = pixelReader[y, x];
            var expected = (r, g, b);
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

        public void Add((int x, int y) pos, int r, int g, int b)
        {
            _colors.Add((pos, (r, g, b)));
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
        var image = TestImageContextFactory.Get().Load(path);
        var actualWidth = image.Width / image.HorizontalResolution;
        var actualHeight = image.Height / image.VerticalResolution;
        Assert.InRange(actualWidth, expectedWidth - DIMENSIONS_THRESHOLD, expectedWidth + DIMENSIONS_THRESHOLD);
        Assert.InRange(actualHeight, expectedHeight - DIMENSIONS_THRESHOLD, expectedHeight + DIMENSIONS_THRESHOLD);
    }
}