using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms.Layout;

namespace NAPS2.EtoForms;

public abstract class EtoPlatform
{
    private static EtoPlatform? _current;

    public static EtoPlatform Current
    {
        get => _current ?? throw new InvalidOperationException();
        set => _current = value ?? throw new ArgumentNullException(nameof(value));
    }

    public virtual bool IsGtk => false;
    public virtual bool IsMac => false;
    public virtual bool IsWinForms => false;

    public abstract Application CreateApplication();
    public abstract IListView<T> CreateListView<T>(ListViewBehavior<T> behavior) where T : notnull;
    public abstract void ConfigureImageButton(Button button);
    public abstract Bitmap ToBitmap(IMemoryImage image);
    public abstract IMemoryImage DrawHourglass(ImageContext imageContext, IMemoryImage thumb);
    public abstract void SetFrame(Control container, Control control, Point location, Size size, bool inOverlay);
    public abstract Control CreateContainer();
    public abstract void AddToContainer(Control container, Control control, bool inOverlay);

    public abstract Control AccessibleImageButton(Image image, string text, Action onClick,
        int xOffset = 0, int yOffset = 0);

    public virtual void SetContainerSize(Window window, Control container, Size size, int padding)
    {
    }

    public virtual Size GetFormSize(Window window)
    {
        return window.Size;
    }

    public virtual void SetFormSize(Window window, Size size)
    {
        window.Size = size;
    }

    public virtual Size GetClientSize(Window window)
    {
        return window.ClientSize;
    }

    public virtual void SetClientSize(Window window, Size clientSize)
    {
        window.ClientSize = clientSize;
    }

    public virtual void SetMinimumClientSize(Window window, Size minSize)
    {
        var windowDecorationSize = window.Size - window.ClientSize;
        window.MinimumSize = minSize + windowDecorationSize;
    }

    public virtual SizeF GetPreferredSize(Control control, SizeF availableSpace)
    {
        return control.GetPreferredSize(availableSpace);
    }

    public virtual LayoutElement FormatProgressBar(ProgressBar progressBar)
    {
        return progressBar.Width(420).Padding(top: 10, bottom: 10);
    }

    public virtual void SetFormLocation(Window window, Point location)
    {
        window.Location = location;
    }

    public virtual void UpdateRtl(Window window)
    {
    }

    public virtual void ConfigureZoomButton(Button button)
    {
    }

    public virtual SizeF GetWrappedSize(Control control, int defaultWidth)
    {
        return control.GetPreferredSize(new SizeF(defaultWidth, LayoutController.MAX_SIZE));
    }

    public virtual void SetClipboardImage(Clipboard clipboard, Bitmap image)
    {
        clipboard.Image = image;
    }

    public virtual void ConfigureDropDown(DropDown dropDown)
    {
    }
}