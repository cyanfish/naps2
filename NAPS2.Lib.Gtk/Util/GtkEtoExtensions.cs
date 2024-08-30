using Gdk;
using Gtk;

namespace NAPS2.Util;

public static class GtkEtoExtensions
{
    public static Image ToScaledImage(this Pixbuf pixbuf, int scaleFactor)
    {
        var surface = Gdk.CairoHelper.SurfaceCreateFromPixbuf(pixbuf, scaleFactor, null);
        return new Image(surface);
    }
}