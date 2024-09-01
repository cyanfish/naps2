using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms.Layout;
using NAPS2.EtoForms.Widgets;

namespace NAPS2.EtoForms;

public abstract class EtoPlatform
{
    private static EtoPlatform? _current;

    public static EtoPlatform Current
    {
        get => _current ?? throw new InvalidOperationException();
        set => _current = value ?? throw new ArgumentNullException(nameof(value));
    }

    protected EtoPlatform()
    {
        ColorScheme = new ColorScheme(DarkModeProvider);
    }

    public virtual bool IsGtk => false;
    public virtual bool IsMac => false;
    public virtual bool IsWinForms => false;

    public abstract IIconProvider IconProvider { get; }
    public abstract IDarkModeProvider DarkModeProvider { get; }
    public ColorScheme ColorScheme { get; }

    public abstract Application CreateApplication();
    public abstract IListView<T> CreateListView<T>(ListViewBehavior<T> behavior) where T : notnull;
    public abstract void ConfigureImageButton(Button button, ButtonFlags flags);
    public abstract Bitmap ToBitmap(IMemoryImage image);
    public abstract IMemoryImage FromBitmap(Bitmap bitmap);
    public abstract IMemoryImage DrawHourglass(IMemoryImage thumb);
    public abstract void SetFrame(Control container, Control control, Point location, Size size, bool inOverlay);
    public abstract Control CreateContainer();
    public abstract void AddToContainer(Control container, Control control, bool inOverlay);
    public abstract void RemoveFromContainer(Control container, Control control);

    public abstract Control AccessibleImageButton(Image image, string text, Action onClick,
        int xOffset = 0, int yOffset = 0);

    public virtual void InitializePlatform()
    {
    }

    public virtual void Invoke(Application application, Action action)
    {
        application.Invoke(action);
    }

    public virtual void AsyncInvoke(Application application, Action action)
    {
        application.AsyncInvoke(action);
    }

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

    public virtual Size GetClientSize(Window window, bool excludeToolbars = false)
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

    public virtual void InitForm(Window window)
    {
    }

    public virtual void ConfigureZoomButton(Button button, string icon)
    {
    }

    public virtual void AttachDpiDependency(Control control, Action<float> callback) => callback(1f);

    public virtual SizeF GetWrappedSize(Control control, int defaultWidth)
    {
        return GetPreferredSize(control, new SizeF(defaultWidth, LayoutController.MAX_SIZE));
    }

    public virtual void SetClipboardImage(Clipboard clipboard, ProcessedImage processedImage, IMemoryImage memoryImage)
    {
        using var etoBitmap = memoryImage.ToEtoImage();
        clipboard.Image = etoBitmap;
    }

    public virtual void ConfigureDropDown(DropDown dropDown)
    {
    }

    public virtual LayoutElement CreateGroupBox(string title, LayoutElement content)
    {
        var groupBox = new GroupBox { Text = title };
        return L.Overlay(groupBox, L.Buffer(content, 6, IsGtk ? 21 : 18, 6, 6));
    }

    public virtual void ShowIcon(Dialog dialog)
    {
    }

    public virtual void ConfigureEllipsis(Label label)
    {
        // TODO: Maybe implement our own ellipsis logic that uses text-measuring to strip trailing characters and add "..."?
    }

    public virtual Bitmap? ExtractAssociatedIcon(string exePath) => throw new NotSupportedException();

    public virtual void HandleKeyDown(Control control, Func<Keys, bool> handle)
    {
        control.KeyDown += (_, args) => args.Handled = handle(args.KeyData);
    }

    public virtual void AttachMouseWheelEvent(Control control, EventHandler<MouseEventArgs> eventHandler)
    {
        control.MouseWheel += eventHandler;
    }

    public virtual void AttachMouseMoveEvent(Control control, EventHandler<MouseEventArgs> eventHandler)
    {
        control.MouseMove += eventHandler;
    }

    public virtual float GetScaleFactor(Window window) => 1f;

    public virtual bool ScaleLayout => false;

    public float GetLayoutScaleFactor(Window window) => ScaleLayout ? GetScaleFactor(window) : 1f;

    public virtual void SetImageSize(ButtonMenuItem menuItem, int size)
    {
    }

    public virtual void SetImageSize(ButtonToolItem toolItem, int size)
    {
    }
}