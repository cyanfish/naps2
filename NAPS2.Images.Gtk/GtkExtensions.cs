using Gdk;

namespace NAPS2.Images.Gtk;

public static class GtkExtensions
{
    public static Pixbuf RenderToPixbuf(this IRenderableImage image)
    {
        var gtkImageContext = image.ImageContext as GtkImageContext ??
                              throw new ArgumentException("The provided image does not have a GtkImageContext");
        return gtkImageContext.RenderToPixbuf(image);
    }

    public static Pixbuf AsPixbuf(this IMemoryImage image)
    {
        var gtkImage = image as GtkImage ?? throw new ArgumentException("Expected a GtkImage", nameof(image));
        return gtkImage.Pixbuf;
    }
}