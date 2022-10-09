using NAPS2.Images.Bitwise;

namespace NAPS2.Images.Storage;

public abstract class AbstractImageTransformer<TImage> where TImage : IMemoryImage
{
    protected AbstractImageTransformer(ImageContext imageContext)
    {
        ImageContext = imageContext;
    }

    protected ImageContext ImageContext { get; }

    public TImage Apply(TImage image, Transform transform)
    {
        if (image.PixelFormat == ImagePixelFormat.Unsupported)
        {
            throw new ArgumentException("Unsupported pixel format for transforms");
        }
        switch (transform)
        {
            case BrightnessTransform brightnessTransform:
                return PerformTransform(image, brightnessTransform);
            case TrueContrastTransform trueContrastTransform:
                return PerformTransform(image, trueContrastTransform);
            case HueTransform hueTransform:
                return PerformTransform(image, hueTransform);
            case SaturationTransform saturationTransform:
                return PerformTransform(image, saturationTransform);
            case SharpenTransform sharpenTransform:
                return PerformTransform(image, sharpenTransform);
            case RotationTransform rotationTransform:
                return PerformTransform(image, rotationTransform);
            case CropTransform cropTransform:
                return PerformTransform(image, cropTransform);
            case ScaleTransform scaleTransform:
                return PerformTransform(image, scaleTransform);
            case BlackWhiteTransform blackWhiteTransform:
                return PerformTransform(image, blackWhiteTransform);
            case ColorBitDepthTransform colorBitDepthTransform:
                return PerformTransform(image, colorBitDepthTransform);
            case ThumbnailTransform thumbnailTransform:
                return PerformTransform(image, thumbnailTransform);
            case CorrectionTransform correctionTransform:
                return PerformTransform(image, correctionTransform);
            default:
                throw new ArgumentException($"Unsupported transform type: {transform.GetType().FullName}");
        }
    }

    private TImage PerformTransform(TImage image, CorrectionTransform transform)
    {
        var stopwatch = Stopwatch.StartNew();
        ColumnColorOp.PerformFullOp(image);
        Console.WriteLine($"Column color op time: {stopwatch.ElapsedMilliseconds}");
        stopwatch.Restart();
        if (transform.Mode == CorrectionMode.Document)
        {
            // We do two filter passes, which is convenient as we can end up with the final data in the original image
            using var image2 = image.CopyBlank();
            new BilateralFilterOp().Perform(image, image2);
            Console.WriteLine($"Bilateral filter op time (pass 1): {stopwatch.ElapsedMilliseconds}");
            stopwatch.Restart();
            WhiteBlackPointOp.PerformFullOp(image2, transform.Mode);
            Console.WriteLine($"White/black point op time: {stopwatch.ElapsedMilliseconds}");
            stopwatch.Restart();
            new BilateralFilterOp().Perform(image2, image);
            Console.WriteLine($"Bilateral filter op time (pass 2): {stopwatch.ElapsedMilliseconds}");
            stopwatch.Restart();
        }
        else
        {
            WhiteBlackPointOp.PerformFullOp(image, transform.Mode);
            Console.WriteLine($"White/black point op time: {stopwatch.ElapsedMilliseconds}");
            stopwatch.Restart();
        }
        return image;
    }

    protected virtual TImage PerformTransform(TImage image, BrightnessTransform transform)
    {
        if (image.PixelFormat is ImagePixelFormat.BW1)
        {
            // No need to handle black & white since brightness is a null transform
            return image;
        }

        float brightnessNormalized = transform.Brightness / 1000f;
        EnsurePixelFormat(ref image);
        // TODO: We need to implement brightness for Gray8
        new BrightnessBitwiseImageOp(brightnessNormalized).Perform(image);
        return image;
    }

    protected virtual TImage PerformTransform(TImage image, TrueContrastTransform transform)
    {
        if (image.PixelFormat is ImagePixelFormat.BW1 or ImagePixelFormat.Gray8)
        {
            // No need to handle grayscale since contrast is a null transform
            return image;
        }

        var contrastNormalized = transform.Contrast / 1000f;
        EnsurePixelFormat(ref image);
        new ContrastBitwiseImageOp(contrastNormalized).Perform(image);
        return image;
    }

    protected virtual TImage PerformTransform(TImage image, HueTransform transform)
    {
        if (image.PixelFormat is ImagePixelFormat.BW1 or ImagePixelFormat.Gray8)
        {
            // No need to handle grayscale since hue shifts are null transforms
            return image;
        }

        float hueShiftNormalized = transform.HueShift / 1000f;
        new HueShiftBitwiseImageOp(hueShiftNormalized).Perform(image);
        return image;
    }

    protected virtual TImage PerformTransform(TImage image, SaturationTransform transform)
    {
        if (image.PixelFormat is ImagePixelFormat.BW1 or ImagePixelFormat.Gray8)
        {
            // No need to handle grayscale since saturation is a null transform
            return image;
        }

        float saturationNormalized = transform.Saturation / 1000f;
        new SaturationBitwiseImageOp(saturationNormalized).Perform(image);
        return image;
    }

    protected virtual TImage PerformTransform(TImage image, SharpenTransform transform)
    {
        if (image.PixelFormat is ImagePixelFormat.BW1 or ImagePixelFormat.Gray8)
        {
            // TODO: Handle grayscale
            return image;
        }

        var newImage = image.CopyBlank();

        float sharpnessNormalized = transform.Sharpness / 1000f;
        new SharpenBitwiseImageOp(sharpnessNormalized).Perform(image, newImage);

        image.Dispose();
        return (TImage) newImage;
    }

    protected abstract TImage PerformTransform(TImage image, RotationTransform transform);

    protected virtual TImage PerformTransform(TImage image, CropTransform transform)
    {
        double xScale = image.Width / (double) (transform.OriginalWidth ?? image.Width),
            yScale = image.Height / (double) (transform.OriginalHeight ?? image.Height);

        int x = Clamp((int) Math.Round(transform.Left * xScale), 0, image.Width - 1);
        int y = Clamp((int) Math.Round(transform.Top * yScale), 0, image.Height - 1);
        int width = Clamp(image.Width - (int) Math.Round((transform.Left + transform.Right) * xScale), 1,
            image.Width - x);
        int height = Clamp(image.Height - (int) Math.Round((transform.Top + transform.Bottom) * yScale), 1,
            image.Height - y);

        var result = ImageContext.Create(width, height, image.PixelFormat);
        result.SetResolution(image.HorizontalResolution, image.VerticalResolution);
        new CopyBitwiseImageOp
        {
            SourceXOffset = x,
            SourceYOffset = y,
            Columns = width,
            Rows = height
        }.Perform(image, result);
        image.Dispose();

        return (TImage) result;
    }

    private int Clamp(int val, int min, int max)
    {
        if (val.CompareTo(min) < 0)
        {
            return min;
        }
        if (val.CompareTo(max) > 0)
        {
            return max;
        }
        return val;
    }

    protected abstract TImage PerformTransform(TImage image, ScaleTransform transform);

    protected virtual TImage PerformTransform(TImage image, BlackWhiteTransform transform)
    {
        if (image.PixelFormat == ImagePixelFormat.BW1)
        {
            return image;
        }

        var monoBitmap = image.CopyBlankWithPixelFormat(ImagePixelFormat.BW1);
        new CopyBitwiseImageOp
        {
            BlackWhiteThreshold = transform.Threshold / 1000f
        }.Perform(image, monoBitmap);
        image.Dispose();

        return (TImage) monoBitmap;
    }

    protected virtual TImage PerformTransform(TImage image, ColorBitDepthTransform transform)
    {
        if (image.PixelFormat is ImagePixelFormat.RGB24 or ImagePixelFormat.ARGB32)
        {
            return image;
        }

        var colorBitmap = image.CopyWithPixelFormat(ImagePixelFormat.RGB24);
        image.Dispose();
        return (TImage) colorBitmap;
    }

    /// <summary>
    /// Gets a bitmap resized to fit within a thumbnail rectangle, including a border around the picture.
    /// </summary>
    /// <param name="image">The bitmap to resize.</param>
    /// <param name="transform">The maximum width and height of the thumbnail.</param>
    /// <returns>The thumbnail bitmap.</returns>
    protected abstract TImage PerformTransform(TImage image, ThumbnailTransform transform);

    /// <summary>
    /// If the provided bitmap is 1-bit (black and white), replace it with a 24-bit bitmap so that image transforms will work. If the bitmap is replaced, the original is disposed.
    /// </summary>
    /// <param name="image">The bitmap that may be replaced.</param>
    protected void EnsurePixelFormat(ref TImage image)
    {
        if (image.PixelFormat == ImagePixelFormat.BW1)
        {
            image = PerformTransform(image, new ColorBitDepthTransform());
        }
    }

    /// <summary>
    /// If the original bitmap is 1-bit (black and white), optimize the result by making it 1-bit too.
    /// </summary>
    /// <param name="original">The original bitmap that is used to determine whether the result should be black and white.</param>
    /// <param name="result">The result that may be replaced.</param>
    protected void OptimizePixelFormat(TImage original, ref TImage result)
    {
        if (original.PixelFormat == ImagePixelFormat.BW1)
        {
            result = PerformTransform(result, new BlackWhiteTransform());
        }
    }
}