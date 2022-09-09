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

    public static void CopyTo(this IMemoryImage source, IMemoryImage destination)
    {
        new CopyBitwiseImageOp().Perform(source, destination);
    }

    public static IMemoryImage CopyWithPixelFormat(this IMemoryImage source, ImagePixelFormat pixelFormat)
    {
        var newImage = source.ImageContext.Create(source.Width, source.Height, pixelFormat);
        newImage.SetResolution(source.HorizontalResolution, source.VerticalResolution);
        new CopyBitwiseImageOp().Perform(source, newImage);
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