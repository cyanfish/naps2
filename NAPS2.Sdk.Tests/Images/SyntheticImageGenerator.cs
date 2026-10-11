global using Rgb = (int r, int g, int b);

using NAPS2.Images.Bitwise;

namespace NAPS2.Sdk.Tests.Images;

/// <summary>
/// Helpers for generating synthetic test images.
/// </summary>
internal static class SyntheticImageGenerator
{
    public static readonly Rgb WHITE = (255, 255, 255);
    public static readonly Rgb GRAY = (128, 128, 128);
    public static readonly Rgb BLACK = (0, 0, 0);

    /// <summary>
    /// Creates an image filled with a background color. The draw callback (if any) runs on an RGB24 image, which is
    /// then converted to the requested pixel format.
    /// </summary>
    public static IMemoryImage CreateSyntheticImage(ImageContext imageContext, ImagePixelFormat pixelFormat,
        int width, int height, Rgb background, Action<IMemoryImage> draw = null)
    {
        var image = imageContext.Create(width, height, ImagePixelFormat.RGB24);
        new FillColorImageOp((byte) background.r, (byte) background.g, (byte) background.b, 255).Perform(image);
        draw?.Invoke(image);
        if (pixelFormat == ImagePixelFormat.RGB24)
        {
            return image;
        }
        using (image)
        {
            return image.CopyWithPixelFormat(pixelFormat);
        }
    }

    /// <summary>
    /// Creates an image filled with a gray background color. See the other overload for details.
    /// </summary>
    public static IMemoryImage CreateSyntheticImage(ImageContext imageContext, ImagePixelFormat pixelFormat,
        int width, int height, int background, Action<IMemoryImage> draw = null) =>
        CreateSyntheticImage(imageContext, pixelFormat, width, height, (background, background, background), draw);

    /// <summary>
    /// Fills the rectangle from (x1, y1) inclusive to (x2, y2) exclusive with a color.
    /// </summary>
    public static void FillRect(IMemoryImage image, int x1, int y1, int x2, int y2, Rgb color)
    {
        using var rect = image.ImageContext.Create(x2 - x1, y2 - y1, ImagePixelFormat.RGB24);
        new FillColorImageOp((byte) color.r, (byte) color.g, (byte) color.b, 255).Perform(rect);
        new CopyBitwiseImageOp { DestXOffset = x1, DestYOffset = y1 }.Perform(rect, image);
    }

    /// <summary>
    /// Fills the rectangle from (x1, y1) inclusive to (x2, y2) exclusive with a gray color.
    /// </summary>
    public static void FillRect(IMemoryImage image, int x1, int y1, int x2, int y2, int color) =>
        FillRect(image, x1, y1, x2, y2, (color, color, color));

    /// <summary>
    /// Draws a page with some dark "text" lines on it.
    /// </summary>
    public static void DrawPage(IMemoryImage image, int x1, int y1, int x2, int y2, int color)
    {
        FillRect(image, x1, y1, x2, y2, color);
        int marginX = (x2 - x1) / 8;
        int marginY = (y2 - y1) / 8;
        for (int y = y1 + marginY; y < y2 - marginY; y += 12)
        {
            FillRect(image, x1 + marginX, y, x2 - marginX, y + 4, 30);
        }
    }

    /// <summary>
    /// Creates a page of mostly near-white noise, with some pure white pixels (including a solid band on the left) and
    /// a dark block. Alpha is randomized too. The output is deterministic for a given size.
    /// </summary>
    public static IMemoryImage CreateNoisyPage(ImageContext imageContext, ImagePixelFormat pixelFormat, int width,
        int height)
    {
        var random = new Random(width * 1000 + height);
        var image = imageContext.Create(width, height, ImagePixelFormat.ARGB32);
        using (image.Lock(LockMode.WriteOnly, out var data))
        {
            unsafe
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        var pixel = data.ptr + data.stride * y + x * data.bytesPerPixel;
                        bool white = random.Next(3) == 0 || x < width / 4;
                        bool dark = !white && x > width / 2 && x < width * 3 / 4 && y > height / 3;
                        *(pixel + data.rOff) = NoiseSample(random, white, dark);
                        *(pixel + data.gOff) = NoiseSample(random, white, dark);
                        *(pixel + data.bOff) = NoiseSample(random, white, dark);
                        *(pixel + data.aOff) = (byte) random.Next(256);
                    }
                }
            }
        }
        if (pixelFormat == ImagePixelFormat.ARGB32)
        {
            return image;
        }
        using (image)
        {
            return image.CopyWithPixelFormat(pixelFormat);
        }
    }

    private static byte NoiseSample(Random random, bool white, bool dark) =>
        white ? (byte) 255 : dark ? (byte) random.Next(0, 80) : (byte) random.Next(180, 256);

    /// <summary>
    /// Returns a copy of the image padded on the right and bottom with a color.
    /// </summary>
    public static IMemoryImage Pad(IMemoryImage image, int padX, int padY, Rgb color)
    {
        var padded = image.ImageContext.Create(image.Width + padX, image.Height + padY, ImagePixelFormat.RGB24);
        padded.SetResolution(image.HorizontalResolution, image.VerticalResolution);
        new FillColorImageOp((byte) color.r, (byte) color.g, (byte) color.b, 255).Perform(padded);
        new CopyBitwiseImageOp().Perform(image, padded);
        return padded;
    }
}
