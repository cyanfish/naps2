using System.Reflection;
using Eto.Forms;
using Eto.GtkSharp;
using Eto.GtkSharp.Forms.Controls;
using Gdk;
using Gtk;

namespace NAPS2.Util;

public static class GtkEtoExtensions
{
    private static readonly FieldInfo GtkImageField =
        typeof(ButtonHandler<Gtk.Button, Eto.Forms.Button, Eto.Forms.Button.ICallback>).GetField("gtkimage",
            BindingFlags.Instance | BindingFlags.NonPublic)!;

    private static readonly MethodInfo SetImagePositionMethod =
        typeof(ButtonHandler<Gtk.Button, Eto.Forms.Button, Eto.Forms.Button.ICallback>).GetMethod(
            "SetImagePosition", BindingFlags.Instance | BindingFlags.NonPublic)!;

    public static Image ToScaledImage(this Pixbuf pixbuf, int scaleFactor)
    {
        // Creating a Gtk.Image directly from a Pixbuf doesn't work if we want to render at high dpi.
        // For example, if we have a 32x32 logical image size that actually gets rendered at 64x64 pixels due to a 2x
        // display scaling, naively using the 64x64 image will get displayed with 64x64 logical size and 128x128
        // physical size, which will look too big and blurry. To get an actual crisp image rendered at 32x32 logical
        // pixels and 64x64 physical pixels, we first create a surface with a 2x scale factor and then create the
        // Gtk.Image from that.
        using var surface = Gdk.CairoHelper.SurfaceCreateFromPixbuf(pixbuf, scaleFactor, null);
        return new Image(surface);
    }

    public static void SetImage(this Eto.Forms.Button button, Image image)
    {
        // Hack to inject a Gtk.Image directly into an Eto.Forms.Button. Normally Eto only takes an Eto.Drawing.Image
        // (which wraps a Pixbuf) but to correctly scale images at high dpi we need a Gtk.Image constructed from a
        // surface.
        image.Show();
        GtkImageField.SetValue(button.Handler, image);
        SetImagePositionMethod.Invoke(button.Handler, []);
    }

    public static void ScaleImage(this Eto.Forms.Button button)
    {
        // Helper to read the Eto.Drawing.Image from the Eto.Forms.Button, scale it, then inject it back into the
        // button.
        int scaleFactor = button.ToNative().ScaleFactor;
        var image = button.Image.ToGdk().ToScaledImage(scaleFactor);
        button.SetImage(image);
    }

    // Workaround for https://github.com/picoe/Eto/issues/2601 with ToEto()
    public static Control AsEto(this Widget widget) => new(new NativeHandler(widget));

    private class NativeHandler : Eto.GtkSharp.Forms.GtkControl<Widget, Control, Control.ICallback>
    {
        public NativeHandler(Widget widget)
        {
            Control = widget;
        }
    }
}