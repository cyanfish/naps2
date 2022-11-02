using System.Reflection;
using Eto;
using Eto.Drawing;
using Eto.Forms;
using Eto.GtkSharp;
using Eto.GtkSharp.Drawing;
using NAPS2.Images.Gtk;
using gtk = Gtk;

namespace NAPS2.EtoForms.Gtk;

public class GtkEtoPlatform : EtoPlatform
{
    // TODO: Can we determine this dynamically? Tried container.GetAllocatedSize.Left/Top which works on LxQT but not Gnome
    private const int X_OFF = 2;
    private const int Y_OFF = 2;
    
    public override bool IsGtk => true;

    public override Application CreateApplication()
    {
        var application = new Application(Platforms.Gtk);
        application.Initialized += (_, _) =>
        {
            // Hack to force Eto to use normal title bars for dialogs
            var type = Assembly.GetAssembly(typeof(Eto.GtkSharp.Platform)).GetType("Eto.GtkSharp.Helper");
            var prop = type.GetField("UseHeaderBar", BindingFlags.Public | BindingFlags.Static);
            prop.SetValue(null, false);
        };
        return application;
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
        var fixedContainer = (gtk.Fixed) container.ToNative();
        fixedContainer.Move(control.ToNative(), location.X - X_OFF, location.Y - Y_OFF);
        control.ToNative().SetSizeRequest(size.Width, size.Height);
    }

    public override Control CreateContainer()
    {
        return new gtk.Fixed().ToEto();
    }

    public override void AddToContainer(Control container, Control control)
    {
        var fixedContainer = (gtk.Fixed) container.ToNative();
        var widget = control.ToNative();
        fixedContainer.Add(widget);
        widget.ShowAll();
    }

    public override void SetContainerSize(Window _window, Control container, Size size, int padding)
    {
        var fixedContainer = (gtk.Fixed) container.ToNative();
        if (!_window.Resizable)
        {
            // This ensures the window has the appropriate margins, otherwise with resizable=false it changes to fit
            // the contents
            fixedContainer.MarginBottom = padding - Y_OFF;
            fixedContainer.MarginEnd = padding - X_OFF;
        }
    }

    public override Size GetFormSize(Window window)
    {
        var gtkWindow = (gtk.Window) window.ToNative();
        gtkWindow.GetSize(out int w, out int h);
        return new Size(w, h);
    }

    public override void SetFormSize(Window window, Size size)
    {
        var gtkWindow = (gtk.Window) window.ToNative();
        gtkWindow.SetDefaultSize(size.Width, size.Height);
    }

    public override SizeF GetPreferredSize(Control control, SizeF availableSpace)
    {
        var widget = control.ToNative();
        if (widget.IsRealized)
        {
            return base.GetPreferredSize(control, availableSpace);
        }
        widget.GetPreferredSize(out var minSize, out var naturalSize);
        return new SizeF(naturalSize.Width, naturalSize.Height);
    }

    public override Size GetClientSize(Window window)
    {
        var gtkWindow = (gtk.Window) window.ToNative();
        gtkWindow.GetSize(out var w, out var h);
        return new Size(w, h);
    }

    public override void SetClientSize(Window window, Size clientSize)
    {
        var gtkWindow = (gtk.Window) window.ToNative();
        gtkWindow.Resize(clientSize.Width, clientSize.Height);
    }

    public override void SetMinimumClientSize(Window window, Size minSize)
    {
        var gtkWindow = (gtk.Window) window.ToNative();
        gtkWindow.SetSizeRequest(minSize.Width, minSize.Height);
    }

    public override void SetFormLocation(Window window, Point location)
    {
        // TODO: Gtk windows drift if we remember location. For now using the default location is fine.
    }

    public override Control AccessibleImageButton(Image image, string text, Action onClick,
        int xOffset = 0, int yOffset = 0)
    {
        var button = new gtk.Button
        {
            // Label = text,
            Image = image.ToGtk(),
            ImagePosition = gtk.PositionType.Left
        };
        button.StyleContext.AddClass("accessible-image-button");
        button.Clicked += (_, _) => onClick();
        return button.ToEto();
    }
}