using System.Drawing;
using System.Drawing.Imaging;

namespace NAPS2.Images.Gdi;

public static class GdiExtensions
{
    public static Bitmap RenderToBitmap(this IRenderableImage image)
    {
        var gdiImageContext = image.ImageContext as GdiImageContext ??
                              throw new ArgumentException("The provided image does not have a GdiImageContext");
        return gdiImageContext.RenderToBitmap(image);
    }

    public static void SafeSetResolution(this Bitmap image, float xDpi, float yDpi)
    {
        if (xDpi > 0 && yDpi > 0)
        {
            image.SetResolution(xDpi, yDpi);
        }
    }

    public static Bitmap AsBitmap(this IMemoryImage image)
    {
        var gdiImage = image as GdiImage ?? throw new ArgumentException("Expected a GdiImage", nameof(image));
        return gdiImage.Bitmap;
    }

    public static ImageFormat AsImageFormat(this ImageFileFormat imageFileFormat)
    {
        switch (imageFileFormat)
        {
            case ImageFileFormat.Bmp:
                return ImageFormat.Bmp;
            case ImageFileFormat.Jpeg:
                return ImageFormat.Jpeg;
            case ImageFileFormat.Png:
                return ImageFormat.Png;
        }
        throw new ArgumentException("Unsupported image format", nameof(imageFileFormat));
    }

    public static ImageFileFormat AsImageFileFormat(this ImageFormat imageFormat)
    {
        if (Equals(imageFormat, ImageFormat.Bmp))
        {
            return ImageFileFormat.Bmp;
        }
        if (Equals(imageFormat, ImageFormat.Jpeg))
        {
            return ImageFileFormat.Jpeg;
        }
        if (Equals(imageFormat, ImageFormat.Png))
        {
            return ImageFileFormat.Png;
        }
        return ImageFileFormat.Unspecified;
    }

    public static PixelFormat AsPixelFormat(this ImagePixelFormat pixelFormat)
    {
        switch (pixelFormat)
        {
            case ImagePixelFormat.BW1:
                return PixelFormat.Format1bppIndexed;
            case ImagePixelFormat.Gray8:
                return PixelFormat.Format8bppIndexed;
            case ImagePixelFormat.RGB24:
                return PixelFormat.Format24bppRgb;
            case ImagePixelFormat.ARGB32:
                return PixelFormat.Format32bppArgb;
        }
        throw new ArgumentException("Unsupported pixel format: " + pixelFormat);
    }

    public static ImagePixelFormat AsImagePixelFormat(this PixelFormat pixelFormat)
    {
        switch (pixelFormat)
        {
            case PixelFormat.Format24bppRgb:
                return ImagePixelFormat.RGB24;
            case PixelFormat.Format32bppArgb:
                return ImagePixelFormat.ARGB32;
            case PixelFormat.Format8bppIndexed:
                return ImagePixelFormat.Gray8;
            case PixelFormat.Format1bppIndexed:
                return ImagePixelFormat.BW1;
            default:
                return ImagePixelFormat.Unsupported;
        }
    }

    public static ImageLockMode AsImageLockMode(this LockMode lockMode)
    {
        switch (lockMode)
        {
            case LockMode.ReadOnly:
                return ImageLockMode.ReadOnly;
            case LockMode.WriteOnly:
                return ImageLockMode.WriteOnly;
            default:
                return ImageLockMode.ReadWrite;
        }
    }
}