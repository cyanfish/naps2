using System.Reflection;
using Eto;
using Eto.Drawing;
using Eto.Forms;
using Eto.GtkSharp;
using Eto.GtkSharp.Drawing;
using NAPS2.EtoForms.Widgets;
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
            var type = Assembly.GetAssembly(typeof(Eto.GtkSharp.Platform))!.GetType("Eto.GtkSharp.Helper");
            var prop = type!.GetField("UseHeaderBar", BindingFlags.Public | BindingFlags.Static);
            prop!.SetValue(null, false);
        };
        return application;
    }

    public override IListView<T> CreateListView<T>(ListViewBehavior<T> behavior) =>
        new GtkListView<T>(behavior);

    public override void ConfigureImageButton(Button button, bool big)
    {
    }

    public override Bitmap ToBitmap(IMemoryImage image)
    {
        var pixbuf = ((GtkImage) image).Pixbuf.Copy();
        return new Bitmap(new BitmapHandler(pixbuf));
    }

    public override void SetClipboardImage(Clipboard clipboard, Bitmap image)
    {
        // Without cloning the image, Gtk gives errors on paste.
        // Presumably it assumes the application will keep the Pixbuf around
        // (while on Windows/Mac we can just dispose right away).
        base.SetClipboardImage(clipboard, image.Clone());
    }

    public override IMemoryImage DrawHourglass(ImageContext imageContext, IMemoryImage image)
    {
        // TODO
        return image;
    }

    public override void SetFrame(Control container, Control control, Point location, Size size, bool inOverlay)
    {
        var overlay = (gtk.Overlay) container.ToNative();
        var widget = control.ToNative();
        if (inOverlay)
        {
            // TODO: Ideally we would use GetChildPosition instead of margin but that signal is not firing, not sure why
            widget.MarginTop = location.Y - Y_OFF;
            widget.MarginStart = location.X - X_OFF;
        }
        else
        {
            var panel = (gtk.Fixed) overlay.Children[0];
            panel.Move(widget, location.X - X_OFF, location.Y - Y_OFF);
        }
        widget.SetSizeRequest(size.Width, size.Height);
        if (widget is gtk.Bin { Child: gtk.Label { Wrap: true } label })
        {
            label.MaxWidthChars = EstimateCharactersWide(size.Width, label);
        }
    }

    public override Control CreateContainer()
    {
        var overlay = new gtk.Overlay();
        overlay.Add(new gtk.Fixed());
        return overlay.ToEto();
    }

    public override void AddToContainer(Control container, Control control, bool inOverlay)
    {
        var overlay = (gtk.Overlay) container.ToNative();
        var widget = control.ToNative();
        if (inOverlay)
        {
            overlay.AddOverlay(widget);
            widget.Halign = gtk.Align.Start;
            widget.Valign = gtk.Align.Start;
        }
        else
        {
            var panel = (gtk.Fixed) overlay.Children[0];
            panel.Add(widget);
        }
        widget.ShowAll();
    }

    public override void RemoveFromContainer(Control container, Control control)
    {
        var overlay = (gtk.Overlay) container.ToNative();
        var widget = control.ToNative();
        overlay.Remove(widget);
        var panel = (gtk.Fixed) overlay.Children[0];
        panel.Remove(widget);
        widget.Unrealize();
    }

    public override void SetContainerSize(Window _window, Control container, Size size, int padding)
    {
        var overlay = (gtk.Overlay) container.ToNative();
        if (!_window.Resizable)
        {
            // This ensures the window has the appropriate margins, otherwise with resizable=false it changes to fit
            // the contents
            overlay.MarginBottom = padding - Y_OFF;
            overlay.MarginEnd = padding - X_OFF;
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
        // TODO: This is a hack to make overlays work. Gtk Overlay uses the margin to adjust the control position but
        // then the margin messes with size calculations. If we set the margin to 0 here (which should be fine as we
        // don't use margin otherwise) then the size calculations normalize, while the overlay has already determined
        // the position so it doesn't affect that any more. However, there is a chance this will break in some edge
        // cases.
        widget.Margin = 0;
        if (widget.IsRealized && widget is not gtk.DrawingArea)
        {
            widget.GetSizeRequest(out var oldWidth, out var oldHeight);
            widget.SetSizeRequest(0, 0);
            try
            {
                return base.GetPreferredSize(control, availableSpace);
            }
            finally
            {
                widget.SetSizeRequest(oldWidth, oldHeight);
            }
        }
        widget.GetPreferredSize(out var minSize, out var naturalSize);
        return new SizeF(naturalSize.Width, naturalSize.Height);
    }

    public override SizeF GetWrappedSize(Control control, int defaultWidth)
    {
        var widget = control.ToNative();
        if (widget is gtk.Bin { Child: gtk.Label label })
        {
            label.MaxWidthChars = EstimateCharactersWide(defaultWidth, label);
            label.GetPreferredSize(out var minSize, out var naturalSize);
            label.GetPreferredHeightForWidth(defaultWidth, out var minHeight, out var naturalHeight);
            return new SizeF(Math.Min(naturalSize.Width + 10, defaultWidth), naturalHeight);
        }
        return base.GetWrappedSize(control, defaultWidth);
    }

    private static int EstimateCharactersWide(int pixelWidth, gtk.Label label)
    {
        // TODO: This could vary based on font and text. Can we do better somehow?
        // Ideally we'd be able to wrap based on a pixel width. Maybe if we put the label in a container?
        var fontSize = label.GetFont().Size / Pango.Scale.PangoScale;
        var approxCharWidth = fontSize * 0.8;
        return (int) Math.Floor(pixelWidth / approxCharWidth);
    }

    public override Size GetClientSize(Window window, bool excludeToolbars)
    {
        var gtkWindow = (gtk.Window) window.ToNative();
        gtkWindow.GetSize(out var w, out var h);
        var size = new Size(w, h);
        if (excludeToolbars && window.ToolBar != null)
        {
            size -= new Size(0, ((gtk.Toolbar) window.ToolBar.ControlObject).AllocatedHeight);
        }
        return size;
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

    public override void ConfigureZoomButton(Button button)
    {
        button.Text = "";
        button.Size = Size.Empty;
        var gtkButton = button.ToNative();
        gtkButton.StyleContext.AddClass("zoom-button");
        gtkButton.SetSizeRequest(0, 0);
    }
}