using Eto.Drawing;
using Eto.Forms;
using Eto.GtkSharp.Drawing;
using NAPS2.Images.Gtk;
using GtkFixed = Gtk.Fixed;

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
        fixedContainer.Add(control.ToNative());
    }
}