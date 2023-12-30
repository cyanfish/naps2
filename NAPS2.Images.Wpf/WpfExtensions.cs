using System.Windows.Media.Imaging;

namespace NAPS2.Images.Wpf;

public static class WpfExtensions
{
    public static BitmapSource RenderToBitmapSource(this IRenderableImage image)
    {
        var wpfImageContext = image.ImageContext as WpfImageContext ??
                              throw new ArgumentException("The provided image does not have a WpfImageContext");
        return wpfImageContext.RenderToBitmapSource(image);
    }

    public static BitmapSource AsBitmapSource(this IMemoryImage image)
    {
        var wpfImage = image as WpfImage ?? throw new ArgumentException("Expected a WpfImage", nameof(image));
        return wpfImage.Bitmap;
    }
}