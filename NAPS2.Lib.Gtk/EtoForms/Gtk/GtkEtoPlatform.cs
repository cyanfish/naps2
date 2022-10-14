using Eto.Drawing;
using Eto.Forms;
using Eto.GtkSharp.Drawing;
using NAPS2.Images.Gtk;
using GtkFixed = Gtk.Fixed;
using GtkWindow = Gtk.Window;

namespace NAPS2.EtoForms.Gtk;

public class GtkEtoPlatform : EtoPlatform
{
    private const int MIN_BUTTON_WIDTH = 75;
    private const int MIN_BUTTON_HEIGHT = 32;
    private const int IMAGE_PADDING = 5;

    static GtkEtoPlatform()
    {
    }

    public override IListView<T> CreateListView<T>(ListViewBehavior<T> behavior) =>
        new GtkListView<T>(behavior);

    public override void ConfigureImageButton(Button button)
    {
    }

    public override Bitmap ToBitmap(IMemoryImage image)
    {
        var pixbuf = ((GtkImage) image).Pixbuf;
        return new Bitmap(new BitmapHandler(pixbuf));
    }

    public override IMemoryImage DrawHourglass(ImageContext imageContext, IMemoryImage image)
    {
        // TODO
        return image;
    }

    public override void SetFrame(Control container, Control control, Point location, Size size)
    {
        var fixedContainer = (GtkFixed) container.ToNative();
        fixedContainer.Move(control.ToNative(), location.X, location.Y);
        control.ToNative().SetSizeRequest(size.Width, size.Height);
    }

    public override Control CreateContainer()
    {
        return new GtkFixed().ToEto();
    }

    public override void AddToContainer(Control container, Control control)
    {
        var fixedContainer = (GtkFixed) container.ToNative();
        var widget = control.ToNative();
        fixedContainer.Add(widget);
        widget.ShowAll();
    }

    public override Size GetFormSize(Window window)
    {
        var gtkWindow = (GtkWindow) window.ToNative();
        gtkWindow.GetSize(out int w, out int h);
        return new Size(w, h);
    }

    public override void SetFormSize(Window window, Size size)
    {
        var gtkWindow = (GtkWindow) window.ToNative();
        gtkWindow.SetDefaultSize(size.Width, size.Height);
    }
}