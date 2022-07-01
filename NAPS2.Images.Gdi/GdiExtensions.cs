using System.Drawing;
using System.Drawing.Imaging;

namespace NAPS2.Images.Gdi;

public static class GdiExtensions
{
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
                // TODO: Maybe it makes sense to have WB1 format too
                return PixelFormat.Format1bppIndexed;
            case ImagePixelFormat.RGB24:
                return PixelFormat.Format24bppRgb;
            case ImagePixelFormat.ARGB32:
                return PixelFormat.Format32bppArgb;
        }
        throw new ArgumentException("Unsupported pixel format: " + pixelFormat);
    }
}
