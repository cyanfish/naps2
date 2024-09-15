using System.Reflection;
using Eto;
using Eto.Drawing;
using Eto.Forms;
using Eto.GtkSharp;
using Eto.GtkSharp.Drawing;
using NAPS2.EtoForms.Widgets;
using NAPS2.Images.Gtk;
using GTK = Gtk;

namespace NAPS2.EtoForms.Gtk;

public class GtkEtoPlatform : EtoPlatform
{
    // TODO: Can we determine this dynamically? Tried container.GetAllocatedSize.Left/Top which works on LxQT but not Gnome
    private const int X_OFF = 2;
    private const int Y_OFF = 2;

    public override bool IsGtk => true;

    public override IIconProvider IconProvider { get; } = new DefaultIconProvider();
    public override IDarkModeProvider DarkModeProvider { get; } = new GtkDarkModeProvider();

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

    public override void ConfigureImageButton(Button button, ButtonFlags flags)
    {
        AttachDpiDependency(button, _ => button.ScaleImage());
    }

    public override Bitmap ToBitmap(IMemoryImage image)
    {
        var pixbuf = ((GtkImage) image).Pixbuf.Copy();
        return new Bitmap(new BitmapHandler(pixbuf));
    }

    public override IMemoryImage FromBitmap(Bitmap bitmap)
    {
        return new GtkImage(bitmap.ToGdk());
    }

    public override void SetClipboardImage(Clipboard clipboard, ProcessedImage processedImage, IMemoryImage memoryImage)
    {
        // We deliberately don't dispose the image here as otherwise Gtk gives errors on paste.
        // Presumably it assumes the application will keep the Pixbuf around
        // (while on Windows/Mac we can just dispose right away).
        clipboard.Image = memoryImage.ToEtoImage();
    }

    public override IMemoryImage DrawHourglass(IMemoryImage image)
    {
        // TODO
        return image;
    }

    public override void SetFrame(Control container, Control control, Point location, Size size, bool inOverlay)
    {
        if (location.X < 0 || location.Y < 0) throw new InvalidOperationException();
        var overlay = container.ToNative() as GTK.Overlay;
        var panel = container.ToNative() as GTK.Fixed;
        var widget = control.ToNative();
        var parent = overlay?.Parent ?? panel?.Parent.Parent;
        int xOff = 0;
        int yOff = 0;
        if (parent is GTK.Alignment)
        {
            // Top-level container, so we offset
            xOff = X_OFF;
            yOff = Y_OFF;
        }
        if (overlay != null)
        {
            // TODO: Ideally we would use GetChildPosition instead of margin but that signal is not firing, not sure why
            widget.MarginTop = location.Y - yOff;
            widget.MarginStart = location.X - xOff;
        }
        if (panel != null)
        {
            if (widget.Parent != panel) throw new InvalidOperationException("Invalid parent");
            panel.Move(widget, location.X - xOff, location.Y - yOff);
        }
        widget.SetSizeRequest(size.Width, size.Height);
        if (widget is GTK.Overlay childOverlay)
        {
            var childPanel = (GTK.Fixed) childOverlay.Child;
            childPanel.SetSizeRequest(size.Width, size.Height);
        }
    }

    public override Control CreateContainer() => new GTK.Fixed().AsEto();

    public override void AddToContainer(Control container, Control control, bool inOverlay)
    {
        var overlay = container.ToNative() as GTK.Overlay;
        var panel = container.ToNative() as GTK.Fixed;
        var widget = control.ToNative();
        if (overlay != null)
        {
            overlay.AddOverlay(widget);
            widget.Halign = GTK.Align.Start;
            widget.Valign = GTK.Align.Start;
        }
        if (panel != null)
        {
            panel.Add(widget);
        }
        widget.ShowAll();
    }
    
    public override Control? MaybeCreateOverlayContainer()
    {
        var overlay = new GTK.Overlay();
        var panel = new GTK.Fixed();
        overlay.Child = panel;
        return overlay.AsEto();
    }

    public override Control? GetOverlayContainer(Control? container, bool inOverlay)
    {
        var overlay = (GTK.Overlay) container.ToNative();
        return inOverlay ? overlay.AsEto() : overlay.Child.AsEto();
    }

    public override void RemoveFromContainer(Control container, Control control)
    {
        var overlay = container.ToNative() as GTK.Overlay;
        var panel = container.ToNative() as GTK.Fixed;
        var widget = control.ToNative();
        overlay?.Remove(widget);
        panel?.Remove(widget);
        widget.Unrealize();
    }

    public override void SetContainerSize(Window _window, Control container, Size size, int padding)
    {
        var native = (GTK.Fixed) container.ToNative();
        if (!_window.Resizable)
        {
            // This ensures the window has the appropriate margins, otherwise with resizable=false it changes to fit
            // the contents
            native.MarginBottom = padding - Y_OFF;
            native.MarginEnd = padding - X_OFF;
        }
    }

    public override Size GetFormSize(Window window)
    {
        var gtkWindow = (GTK.Window) window.ToNative();
        gtkWindow.GetSize(out int w, out int h);
        return new Size(w, h);
    }

    public override void SetFormSize(Window window, Size size)
    {
        var gtkWindow = (GTK.Window) window.ToNative();
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
        if (control is ImageView && control.Size.Width > 1)
        {
            return control.Size;
        }
        if (widget.IsRealized && widget is not GTK.DrawingArea)
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
        if (widget is GTK.Bin { Child: GTK.Label label })
        {
            label.MaxWidthChars = EstimateCharactersWide(defaultWidth, label);
            label.GetPreferredSize(out var minSize, out var naturalSize);
            label.GetPreferredHeightForWidth(defaultWidth, out var minHeight, out var naturalHeight);
            return new SizeF(Math.Min(naturalSize.Width + 10, defaultWidth), naturalHeight);
        }
        return base.GetWrappedSize(control, defaultWidth);
    }

    private static int EstimateCharactersWide(int pixelWidth, GTK.Label label)
    {
        // TODO: This could vary based on font and text. Can we do better somehow?
        // Ideally we'd be able to wrap based on a pixel width. Maybe if we put the label in a container?
        var fontSize = label.GetFont().Size / Pango.Scale.PangoScale;
        var approxCharWidth = fontSize * 0.75;
        return (int) Math.Floor(pixelWidth / approxCharWidth);
    }

    public override void ConfigureEllipsis(Label label)
    {
        var eventBox = (GTK.EventBox) label.ToNative();
        var gtkLabel = (GTK.Label) eventBox.Child;
        gtkLabel.Ellipsize = Pango.EllipsizeMode.End;
    }

    public override Size GetClientSize(Window window, bool excludeToolbars)
    {
        var gtkWindow = (GTK.Window) window.ToNative();
        gtkWindow.GetSize(out var w, out var h);
        var size = new Size(w, h);
        if (excludeToolbars && window.ToolBar != null)
        {
            var toolbar = (GTK.Toolbar) window.ToolBar.ControlObject;
            var vbox = (GTK.VBox) toolbar.Parent;
            var heights = vbox.Children.OfType<GTK.Toolbar>().Select(x =>
            {
                x.GetPreferredHeight(out _, out int naturalHeight);
                return naturalHeight;
            });
            size -= new Size(0, heights.Sum());
        }
        return size;
    }

    public override void SetClientSize(Window window, Size clientSize)
    {
        var gtkWindow = (GTK.Window) window.ToNative();
        gtkWindow.Resize(clientSize.Width, clientSize.Height);
    }

    public override void SetMinimumClientSize(Window window, Size minSize)
    {
        var gtkWindow = (GTK.Window) window.ToNative();
        gtkWindow.SetSizeRequest(minSize.Width, minSize.Height);
    }

    public override void SetFormLocation(Window window, Point location)
    {
        // TODO: Gtk windows drift if we remember location. For now using the default location is fine.
    }

    public override float GetScaleFactor(Window window)
    {
        // GTK scale factors are integers. Any fractional scaling (e.g. 1.5x) works by rendering at 2x and then scaling
        // down.
        return window.ToNative().ScaleFactor;
    }

    public override void AttachDpiDependency(Control control, Action<float> callback)
    {
        if (control.Loaded)
        {
            callback(GetScaleFactor(control.ParentWindow));
        }
        else
        {
            control.Load += (_, _) => callback(GetScaleFactor(control.ParentWindow));
        }
    }

    public override void ConfigureDonateButton(Button button)
    {
        var native = (GTK.Button) button.ToNative();
        native.StyleContext.AddClass("donate-button");
    }

    public override void ConfigureZoomButton(Button button, string icon)
    {
        var gtkButton = button.ToNative();
        button.Text = "";
        button.Image = IconProvider.GetIcon(icon, gtkButton.ScaleFactor);
        button.ScaleImage();
        button.Size = Size.Empty;
        gtkButton.StyleContext.AddClass("zoom-button");
        gtkButton.SetSizeRequest(0, 0);
    }

    public override void AttachMouseWheelEvent(Control control, EventHandler<MouseEventArgs> eventHandler)
    {
        var native = control.ToNative();
        // Attach to the child so that the scrollbars don't steal the event from us
        if (native is GTK.EventBox eventBox)
        {
            native = eventBox.Child;
        }
        if (native is GTK.ScrolledWindow scrolledWindow)
        {
            native = scrolledWindow.Child;
        }
        native.ScrollEvent += (sender, args) =>
        {
            var ev = args.Event;
            var newArgs = new MouseEventArgs(
                MouseButtons.None,
                ev.State.ToEtoKey(),
                new PointF((float) ev.X, (float) ev.Y),
                // Negate deltaY to match WinForms
                new SizeF((float) ev.DeltaX, (float) -ev.DeltaY));
            eventHandler.Invoke(sender, newArgs);
            args.RetVal = newArgs.Handled;
        };
    }
}