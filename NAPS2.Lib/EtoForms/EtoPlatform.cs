using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms.Layout;
using NAPS2.EtoForms.Widgets;

namespace NAPS2.EtoForms;

public abstract class EtoPlatform
{
    private static EtoPlatform? _current;

    public static bool HasCurrent => _current != null;

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
    public virtual bool IsWpf => false;
    public bool IsWindows => IsWinForms || IsWpf;

    public abstract IIconProvider IconProvider { get; }
    public abstract IDarkModeProvider DarkModeProvider { get; }
    public ColorScheme ColorScheme { get; }

    public Application CreateApplication()
    {
        var app = CreateApplicationCore();
        app.UnhandledException += UnhandledException;
        return app;
    }

    private static void UnhandledException(object? sender, Eto.UnhandledExceptionEventArgs e)
    {
        Log.FatalException("An error occurred that caused the application to close.",
            e.ExceptionObject as Exception ?? new Exception());
    }

    public abstract Application CreateApplicationCore();
    public abstract void SetSystemTheme();
    public abstract IListView<T> CreateListView<T>(ListViewBehavior<T> behavior) where T : notnull;
    public abstract ISettingsNavigator CreateSettingsNavigator();

    public virtual bool SupportsWindowSidebar => false;
    public virtual void AttachWindowSidebar(Window window, Control sidebar, WindowSidebarOptions options) =>
        throw new NotSupportedException();
    public virtual void SetWindowSidebarVisible(Window window, bool visible) =>
        throw new NotSupportedException();

    public abstract void ConfigureImageButton(Button button, ButtonFlags flags);
    public abstract Bitmap ToBitmap(IMemoryImage image);
    public abstract IMemoryImage FromBitmap(Bitmap bitmap);
    public abstract IMemoryImage DrawHourglass(IMemoryImage thumb);
    public abstract void SetFrame(Control container, Control control, Point location, Size size, bool inOverlay);
    public abstract Control CreateContainer();
    public abstract void AddToContainer(Control container, Control control, bool inOverlay);
    public abstract void RemoveFromContainer(Control container, Control control);

    public abstract Control CreateScrollContainer();
    public abstract Control GetScrollContentContainer(Control scrollContainer);
    public abstract void SetScrollContentSize(Control scrollContainer, Size size);
    public abstract int GetVerticalScrollBarWidth(Control scrollContainer);

    public abstract void ConfigureDonateButton(Button button);

    public virtual void InitializeForegroundApp()
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

    public virtual Size GetContainerSize(Control container) => container.Size;

    public virtual void AttachContainerSizeChanged(Window window, Control container, Action action)
    {
        container.SizeChanged += (_, _) => action();
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

    public virtual SizeF GetMinimumSize(Control control) => SizeF.Empty;

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

    public virtual void SetAccessibleName(Control control, string name)
    {
    }

    public virtual void AttachDpiDependency(Control control, Action<float> callback) =>
        callback(GetScaleFactor(control.ParentWindow));

    public virtual SizeF GetWrappedSize(Control control, int defaultWidth)
    {
        return GetPreferredSize(control, new SizeF(defaultWidth, LayoutController.MAX_SIZE));
    }

    public virtual void SetClipboardImage(Clipboard clipboard, ProcessedImage processedImage, IMemoryImage memoryImage)
    {
        using var etoBitmap = memoryImage.ToEtoImage();
        clipboard.Image = etoBitmap;
    }

    public virtual void ConfigureDropDown(DropDown dropDown, bool scale)
    {
    }

    public virtual LayoutElement CreateGroupBox(string title, LayoutElement content, GroupBoxHeaderStyle headerStyle)
    {
        var groupBox = new GroupBox { Text = title };
        return L.Overlay(groupBox, L.Buffer(content, 6, IsGtk ? 21 : 18, 6, 6));
    }

    public virtual void ShowIcon(Dialog dialog)
    {
    }

    public virtual void ConfigureSmallToolBar(ToolBar toolBar)
    {
    }

    public virtual void ConfigureEllipsis(Label label)
    {
        // TODO: Maybe implement our own ellipsis logic that uses text-measuring to strip trailing characters and add "..."?
    }

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

    public virtual void SetImageSize(ToolItem toolItem, int size)
    {
    }

    public virtual void SetImageSize(ImageView imageView, int size)
    {
    }

    public virtual Control? MaybeCreateOverlayContainer() => null;

    public virtual Control? GetOverlayContainer(Control? container, bool inOverlay) => null;

    public virtual void SetSplitterPosition(Splitter splitter, int pos) => splitter.Position = pos;

    public virtual void ConfigureFileDialog(FileDialog fileDialog)
    {
    }

    public virtual byte[] GetData(IDataObject dataObject, string typeName)
    {
        return dataObject.GetData(typeName);
    }
}