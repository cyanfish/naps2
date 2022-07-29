namespace NAPS2.Images.Storage;

public abstract class AbstractImageTransformer<TImage> where TImage : IMemoryImage
{
    private readonly ImageContext _imageContext;

    protected AbstractImageTransformer(ImageContext imageContext)
    {
        _imageContext = imageContext;
    }

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
            case ContrastTransform contrastTransform:
                return PerformTransform(image, contrastTransform);
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
            default:
                throw new ArgumentException($"Unsupported transform type: {transform.GetType().FullName}");
        }
    }

    protected virtual TImage PerformTransform(TImage image, BrightnessTransform transform)
    {
        float brightnessAdjusted = transform.Brightness / 1000f;
        EnsurePixelFormat(ref image);
        UnsafeImageOps.ChangeBrightness(image, brightnessAdjusted);
        return image;
    }

    protected abstract TImage PerformTransform(TImage image, ContrastTransform transform);

    protected virtual TImage PerformTransform(TImage image, TrueContrastTransform transform)
    {
        // convert +/-1000 input range to a logarithmic scaled multiplier
        float contrastAdjusted = (float)Math.Pow(2.718281f, transform.Contrast / 500.0f);
        // see http://docs.rainmeter.net/tips/colormatrix-guide/ for offset & matrix calculation
        float offset = (1.0f - contrastAdjusted) / 2.0f;

        EnsurePixelFormat(ref image);
        UnsafeImageOps.ChangeContrast(image, contrastAdjusted, offset);
        return image;
    }

    protected virtual TImage PerformTransform(TImage image, HueTransform transform)
    {
        if (image.PixelFormat == ImagePixelFormat.BW1)
        {
            // No need to handle 1bpp since hue shifts are null transforms
            return image;
        }

        float hueShiftAdjusted = transform.HueShift / 2000f * 360;
        if (hueShiftAdjusted < 0)
        {
            hueShiftAdjusted += 360;
        }

        UnsafeImageOps.HueShift(image, hueShiftAdjusted);

        return image;
    }

    protected abstract TImage PerformTransform(TImage image, SaturationTransform transform);

    protected abstract TImage PerformTransform(TImage image, SharpenTransform transform);

    protected abstract TImage PerformTransform(TImage image, RotationTransform transform);

    protected abstract TImage PerformTransform(TImage image, CropTransform transform);

    protected abstract TImage PerformTransform(TImage image, ScaleTransform transform);

    protected virtual TImage PerformTransform(TImage image, BlackWhiteTransform transform)
    {
        if (image.PixelFormat == ImagePixelFormat.BW1)
        {
            return image;
        }

        var monoBitmap = UnsafeImageOps.ConvertTo1Bpp(image, transform.Threshold, _imageContext);
        image.Dispose();

        return (TImage) monoBitmap;
    }

    protected virtual TImage PerformTransform(TImage image, ColorBitDepthTransform transform)
    {
        if (image.PixelFormat != ImagePixelFormat.BW1)
        {
            return image;
        }

        var colorBitmap = UnsafeImageOps.ConvertTo24Bpp(image, _imageContext);
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