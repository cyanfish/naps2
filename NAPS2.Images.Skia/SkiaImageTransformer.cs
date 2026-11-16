using NAPS2.Images.Bitwise;
using SkiaSharp;

namespace NAPS2.Images.Skia;

internal class SkiaImageTransformer : AbstractImageTransformer<SkiaImage>
{
    public SkiaImageTransformer(ImageContext imageContext) : base(imageContext)
    {
    }

    protected override SkiaImage PerformTransform(SkiaImage image, RotationTransform transform)
    {
        int width = image.Width, height = image.Height;
        float xres = image.HorizontalResolution, yres = image.VerticalResolution;
        if (transform.Angle is > 45.0 and < 135.0 or > 225.0 and < 315.0)
        {
            (width, height) = (height, width);
            (xres, yres) = (yres, xres);
        }

        var newImage = Draw(image, width, height, canvas =>
        {
            canvas.Clear(SKColors.White);
            canvas.Translate(width / 2f, height / 2f);
            canvas.RotateDegrees((float) transform.Angle);
            canvas.Translate(-image.Width / 2f, -image.Height / 2f);
            using var source = SKImage.FromPixels(image.Bitmap.PeekPixels());
            canvas.DrawImage(source, 0, 0, new SKSamplingOptions(SKFilterMode.Linear));
        });
        OptimizePixelFormat(image, ref newImage);
        newImage.LogicalPixelFormat = image.LogicalPixelFormat;
        newImage.SetResolution(xres, yres);
        image.Dispose();
        return newImage;
    }

    protected override SkiaImage PerformTransform(SkiaImage image, ResizeTransform transform)
    {
        if (transform.Width == image.Width && transform.Height == image.Height)
        {
            return image;
        }
        bool isDownscale = transform.Width < image.Width || transform.Height < image.Height;
        var sampling = isDownscale
            ? new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear)
            : new SKSamplingOptions(SKCubicResampler.Mitchell);

        var newImage = Draw(image, transform.Width, transform.Height, canvas =>
        {
            using var source = SKImage.FromPixels(image.Bitmap.PeekPixels());
            canvas.DrawImage(source, new SKRect(0, 0, transform.Width, transform.Height), sampling);
        });
        newImage.LogicalPixelFormat = image.LogicalPixelFormat == ImagePixelFormat.BW1
            ? ImagePixelFormat.Gray8
            : image.LogicalPixelFormat;
        newImage.SetResolution(
            image.HorizontalResolution * image.Width / transform.Width,
            image.VerticalResolution * image.Height / transform.Height);
        image.Dispose();
        return newImage;
    }

    protected override SkiaImage PerformTransform(SkiaImage image, BlackWhiteTransform transform)
    {
        new DecolorBitwiseImageOp(true)
        {
            BlackWhiteThreshold = (transform.Threshold + 1000) / 2000f
        }.Perform(image);
        var newImage = (SkiaImage) image.CopyWithPixelFormat(ImagePixelFormat.Gray8);
        newImage.LogicalPixelFormat = ImagePixelFormat.BW1;
        image.Dispose();
        return newImage;
    }

    /// <summary>
    /// Draws to a new image with the same pixel format as the original. Skia can only draw to premultiplied surfaces,
    /// so this draws to an intermediate surface and then converts to the storage format.
    /// </summary>
    private static SkiaImage Draw(SkiaImage original, int width, int height, Action<SKCanvas> draw)
    {
        using var surface = SKSurface.Create(
                                new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul)) ??
                            throw new InvalidOperationException("Could not create Skia surface");
        draw(surface.Canvas);
        var bitmap = new SKBitmap(new SKImageInfo(width, height, original.Bitmap.ColorType, original.Bitmap.AlphaType));
        try
        {
            if (!surface.ReadPixels(bitmap.Info, bitmap.GetPixels(), bitmap.RowBytes, 0, 0))
            {
                throw new InvalidOperationException("Could not read pixels from Skia surface");
            }
            return new SkiaImage(bitmap)
            {
                OriginalFileFormat = original.OriginalFileFormat
            };
        }
        catch
        {
            bitmap.Dispose();
            throw;
        }
    }
}
