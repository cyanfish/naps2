using NAPS2.Images.Bitwise;

namespace NAPS2.Images;

public static class ImageExtensions
{
    public static IMemoryImage Render(this IRenderableImage image)
    {
        return image.ImageContext.Render(image);
    }

    public static IMemoryImage PerformTransform(this IMemoryImage image, Transform transform)
    {
        return image.ImageContext.PerformTransform(image, transform);
    }

    public static IMemoryImage PerformAllTransforms(this IMemoryImage image, IEnumerable<Transform> transforms)
    {
        return image.ImageContext.PerformAllTransforms(image, transforms);
    }

    public static void UpdateLogicalPixelFormat(this IMemoryImage image)
    {
        var op = new LogicalPixelFormatOp();
        op.Perform(image);
        image.LogicalPixelFormat = op.LogicalPixelFormat;
    }

    /// <summary>
    /// Copies the content of this image to the destination image. It does not need to be the same pixel format, but if it's different,
    /// there may be some loss of information (e.g. when converting color to gray or black/white).
    /// </summary>
    /// <param name="source"></param>
    /// <param name="destination"></param>
    public static void CopyTo(this IMemoryImage source, IMemoryImage destination)
    {
        new CopyBitwiseImageOp().Perform(source, destination);
    }

    /// <summary>
    /// Creates a new image with the same content, dimensions, and resolution as this image.
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static IMemoryImage Copy(this IMemoryImage source)
    {
        return source.CopyWithPixelFormat(source.PixelFormat);
    }

    /// <summary>
    /// Creates a new image with the same content, dimensions, and resolution as this image, but possibly with a different pixel format.
    /// This can result in some loss of information (e.g. when converting color to gray or black/white).
    /// </summary>
    /// <param name="source"></param>
    /// <param name="pixelFormat"></param>
    /// <returns></returns>
    public static IMemoryImage CopyWithPixelFormat(this IMemoryImage source, ImagePixelFormat pixelFormat)
    {
        if (pixelFormat == ImagePixelFormat.Unsupported) throw new ArgumentException();
        var newImage = source.CopyBlankWithPixelFormat(pixelFormat);
        new CopyBitwiseImageOp().Perform(source, newImage);
        newImage.OriginalFileFormat = source.OriginalFileFormat;
        if (source.LogicalPixelFormat < pixelFormat)
        {
            newImage.LogicalPixelFormat = source.LogicalPixelFormat;
        }
        return newImage;
    }

    /// <summary>
    /// Creates a new (empty) image with the same dimensions, pixel format, and resolution as this image.
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static IMemoryImage CopyBlank(this IMemoryImage source)
    {
        return source.CopyBlankWithPixelFormat(source.PixelFormat);
    }

    /// <summary>
    /// Creates a new (empty) image with the same dimensions and resolution as this image, and the specified pixel format.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="pixelFormat"></param>
    /// <returns></returns>
    public static IMemoryImage CopyBlankWithPixelFormat(this IMemoryImage source, ImagePixelFormat pixelFormat)
    {
        if (pixelFormat == ImagePixelFormat.Unsupported) throw new ArgumentException();
        var newImage = source.ImageContext.Create(source.Width, source.Height, pixelFormat);
        newImage.SetResolution(source.HorizontalResolution, source.VerticalResolution);
        return newImage;
    }

    public static MemoryStream SaveToMemoryStream(this IMemoryImage image, ImageFileFormat imageFormat,
        int quality = -1)
    {
        var stream = new MemoryStream();
        image.Save(stream, imageFormat, quality);
        stream.Seek(0, SeekOrigin.Begin);
        return stream;
    }

    public static string AsTypeHint(this ImageFileFormat imageFormat)
    {
        return imageFormat switch
        {
            ImageFileFormat.Bmp => ".bmp",
            ImageFileFormat.Jpeg => ".jpg",
            ImageFileFormat.Png => ".png",
            _ => ""
        };
    }
}