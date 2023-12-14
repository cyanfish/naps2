using System.Diagnostics.CodeAnalysis;
using NAPS2.Images.Bitwise;

namespace NAPS2.Images;

public static class ImageExtensions
{
    public static IMemoryImage Render(this IRenderableImage image)
    {
        return image.ImageContext.Render(image);
    }

    /// <summary>
    /// Checks if we can copy the source JPEG directly rather than re-encoding and suffering JPEG degradation.
    /// </summary>
    /// <param name="image"></param>
    /// <param name="jpegPath"></param>
    /// <returns></returns>
    internal static bool IsUntransformedJpegFile(this IRenderableImage image,
        [MaybeNullWhen(false)] out string jpegPath)
    {
        if (image is { Storage: ImageFileStorage fileStorage, TransformState.IsEmpty: true } &&
            ImageContext.GetFileFormatFromExtension(fileStorage.FullPath) == ImageFileFormat.Jpeg)
        {
            jpegPath = fileStorage.FullPath;
            return true;
        }
        jpegPath = null;
        return false;
    }

    /// <summary>
    /// Saves the image to the given file path. If the file format is unspecified, it will be inferred from the
    /// file extension if possible.
    /// </summary>
    /// <param name="image">The image to save.</param>
    /// <param name="path">The path to save the image file to.</param>
    /// <param name="imageFormat">The file format to use.</param>
    /// <param name="options">Options for saving, e.g. JPEG quality.</param>
    public static void Save(this IRenderableImage image, string path,
        ImageFileFormat imageFormat = ImageFileFormat.Unspecified, ImageSaveOptions? options = null)
    {
        if (imageFormat == ImageFileFormat.Unspecified)
        {
            imageFormat = ImageContext.GetFileFormatFromExtension(path);
        }
        if (imageFormat == ImageFileFormat.Jpeg && image.IsUntransformedJpegFile(out var jpegPath))
        {
            File.Copy(jpegPath, path);
            return;
        }
        using var renderedImage = image.Render();
        renderedImage.Save(path, imageFormat, options);
    }

    /// <summary>
    /// Saves the image to the given stream. The file format must be specified.
    /// </summary>
    /// <param name="image">The image to save.</param>
    /// <param name="stream">The stream to save the image to.</param>
    /// <param name="imageFormat">The file format to use.</param>
    /// <param name="options">Options for saving, e.g. JPEG quality.</param>
    public static void Save(this IRenderableImage image, Stream stream,
        ImageFileFormat imageFormat = ImageFileFormat.Unspecified, ImageSaveOptions? options = null)
    {
        if (imageFormat == ImageFileFormat.Unspecified)
        {
            throw new ArgumentException("Format required to save to a stream", nameof(imageFormat));
        }
        if (imageFormat == ImageFileFormat.Jpeg && image.IsUntransformedJpegFile(out var jpegPath))
        {
            using var fileStream = File.OpenRead(jpegPath);
            fileStream.CopyTo(stream);
            return;
        }
        using var renderedImage = image.Render();
        renderedImage.Save(stream, imageFormat, options);
    }

    /// <summary>
    /// Saves the image to a new MemoryStream object. The file format must be specified.
    /// </summary>
    /// <param name="image">The image to save.</param>
    /// <param name="imageFormat">The file format to use.</param>
    /// <param name="options">Options for saving, e.g. JPEG quality.</param>
    public static MemoryStream SaveToMemoryStream(this IMemoryImage image, ImageFileFormat imageFormat,
        ImageSaveOptions? options = null)
    {
        var stream = new MemoryStream();
        image.Save(stream, imageFormat, options);
        stream.Seek(0, SeekOrigin.Begin);
        return stream;
    }

    /// <summary>
    /// Saves the image to a new MemoryStream object. The file format must be specified.
    /// </summary>
    /// <param name="image">The image to save.</param>
    /// <param name="imageFormat">The file format to use.</param>
    /// <param name="options">Options for saving, e.g. JPEG quality.</param>
    public static MemoryStream SaveToMemoryStream(this IRenderableImage image, ImageFileFormat imageFormat,
        ImageSaveOptions? options = null)
    {
        using var renderedImage = image.Render();
        return renderedImage.SaveToMemoryStream(imageFormat, options);
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
    /// Creates a new image with the same content, dimensions, and resolution as this image.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="imageContext"></param>
    /// <returns></returns>
    public static IMemoryImage Copy(this IMemoryImage source, ImageContext imageContext)
    {
        return source.CopyWithPixelFormat(imageContext, source.PixelFormat);
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
        return source.CopyWithPixelFormat(source.ImageContext, pixelFormat);
    }

    /// <summary>
    /// Creates a new image with the same content, dimensions, and resolution as this image, but possibly with a different pixel format.
    /// This can result in some loss of information (e.g. when converting color to gray or black/white).
    /// </summary>
    /// <param name="source"></param>
    /// <param name="imageContext"></param>
    /// <param name="pixelFormat"></param>
    /// <returns></returns>
    public static IMemoryImage CopyWithPixelFormat(this IMemoryImage source, ImageContext imageContext,
        ImagePixelFormat pixelFormat)
    {
        if (pixelFormat == ImagePixelFormat.Unsupported) throw new ArgumentException();
        var newImage = source.CopyBlankWithPixelFormat(imageContext, pixelFormat);
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
        return CopyBlankWithPixelFormat(source, source.ImageContext, pixelFormat);
    }

    /// <summary>
    /// Creates a new (empty) image with the same dimensions and resolution as this image, and the specified pixel format.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="imageContext"></param>
    /// <param name="pixelFormat"></param>
    /// <returns></returns>
    public static IMemoryImage CopyBlankWithPixelFormat(this IMemoryImage source, ImageContext imageContext,
        ImagePixelFormat pixelFormat)
    {
        if (pixelFormat == ImagePixelFormat.Unsupported) throw new ArgumentException();
        var newImage = imageContext.Create(source.Width, source.Height, pixelFormat);
        newImage.SetResolution(source.HorizontalResolution, source.VerticalResolution);
        return newImage;
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