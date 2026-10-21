using System.Windows.Media.Imaging;

namespace NAPS2.Images.Wpf;

public static class WpfImageExtensions
{
    /// <summary>
    /// Renders the given image to a BitmapSource matching the original image's DPI.
    /// </summary>
    public static BitmapSource RenderToBitmapSource(this IRenderableImage image)
    {
        var wpfImageContext = image.ImageContext as WpfImageContext ??
                              throw new ArgumentException("The provided image does not have a WpfImageContext");
        return wpfImageContext.RenderToBitmapSource(image);
    }

    /// <summary>
    /// Renders the given image to a BitmapSource with fixed 96 DPI (suitable for showing on screen).
    /// </summary>
    public static BitmapSource RenderToDisplayBitmapSource(this IRenderableImage image)
    {
        var wpfImageContext = image.ImageContext as WpfImageContext ??
                              throw new ArgumentException("The provided image does not have a WpfImageContext");
        return wpfImageContext.RenderToDisplayBitmapSource(image);
    }

    /// <summary>
    /// Copies the given WpfImage to a BitmapSource matching the original image's DPI.
    /// </summary>
    public static BitmapSource ToBitmapSource(this IMemoryImage image)
    {
        var wpfImage = image as WpfImage ?? throw new ArgumentException("Expected a WpfImage", nameof(image));
        return wpfImage.ToBitmapSource();
    }

    /// <summary>
    /// Copies the given WpfImage to a BitmapSource with fixed 96 DPI (suitable for showing on screen).
    /// </summary>
    public static BitmapSource ToDisplayBitmapSource(this IMemoryImage image)
    {
        var wpfImage = image as WpfImage ?? throw new ArgumentException("Expected a WpfImage", nameof(image));
        return wpfImage.ToDisplayBitmapSource();
    }

    [Obsolete("Use ToBitmapSource instead. This has been renamed since it now returns a copy of the image.")]
    public static BitmapSource AsBitmapSource(this IMemoryImage image) => image.ToBitmapSource();
}