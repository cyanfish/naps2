using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms.Layout;
using NAPS2.EtoForms.Widgets;

namespace NAPS2.EtoForms.Ui;

/// <summary>
/// An interactive editing operation on an image, with a live preview and controls. The preview is shown on a
/// ScrollZoomImageViewer provided by the host, which the editor also draws its overlay on and gets mouse events from.
/// </summary>
public abstract class ImageEditor : IDisposable
{
    private readonly HelpWidget _help = new();
    private readonly RefreshThrottle _renderThrottle;
    private ScrollZoomImageViewer? _viewer;
    private Bitmap? _viewerBitmap;

    protected ImageEditor(Naps2Config config, UiImageList imageList, ThumbnailController thumbnailController)
    {
        Config = config;
        ImageList = imageList;
        ThumbnailController = thumbnailController;
        _renderThrottle = new RefreshThrottle(RenderImage);
        OkButton = C.Button(UiStrings.OK, () =>
        {
            Apply();
            Finished?.Invoke(this, EventArgs.Empty);
        });
        CancelButton = C.Button(UiStrings.Cancel, () => Finished?.Invoke(this, EventArgs.Empty));
    }

    public UiImage Image { get; set; } = null!;
    public List<UiImage> SelectedImages
    {
        get;
        set
        {
            field = value;
            if (IsInitialized)
            {
                OnSelectedImagesChanged();
            }
        }
    } = null!;

    public Window? ParentWindow { get; set; }

    public ScrollZoomImageViewer Viewer
    {
        get => _viewer ?? throw new InvalidOperationException("Viewer must be set before the editor is initialized");
        set => _viewer = value;
    }

    public string Title { get; protected set; } = "";
    public string? IconName { get; protected set; }

    public Button OkButton { get; }
    public Button CancelButton { get; }

    /// <summary>
    /// Raised when the user accepts (after the changes are applied) or cancels the edit.
    /// </summary>
    public event EventHandler? Finished;

    protected Naps2Config Config { get; }
    protected UiImageList ImageList { get; }
    protected ThumbnailController ThumbnailController { get; }

    protected bool IsInitialized { get; private set; }

    protected IMemoryImage? DisplayImage { get; set; }
    protected string? HelpText { get; set; }

    protected virtual int ImagePadding => 0;

    // The image's bounds in the viewer's coordinate space (as used for painting the overlay and for mouse events)
    protected float ViewL => Viewer.ImageRect.Left;
    protected float ViewT => Viewer.ImageRect.Top;
    protected float ViewR => Viewer.ImageRect.Right;
    protected float ViewB => Viewer.ImageRect.Bottom;
    protected float ViewW => Viewer.ImageRect.Width;
    protected float ViewH => Viewer.ImageRect.Height;

    protected Cursor? Cursor
    {
        set => Viewer.OverlayCursor = value;
    }

    /// <summary>
    /// Renders the initial preview. Image, SelectedImages and Viewer must be set first.
    /// </summary>
    public void Initialize()
    {
        var viewer = Viewer;
        viewer.PanEnabled = false;
        viewer.FitIsMinZoom = true;
        viewer.ImagePadding = ImagePadding;
        viewer.PaintOverlay += PaintOverlay;
        viewer.OverlayMouseDown += ViewerMouseDown;
        viewer.OverlayMouseMove += ViewerMouseMove;
        viewer.OverlayMouseUp += ViewerMouseUp;
        OnInitialize();
        IsInitialized = true;
        ShowDisplayImage();
        viewer.ZoomToContainer();
        UpdatePreviewBox();
    }

    protected virtual void OnInitialize()
    {
        InitDisplayImage();
    }

    protected virtual void OnSelectedImagesChanged()
    {
    }

    /// <summary>
    /// Creates the layout for the editor's controls (the host lays out the viewer).
    /// </summary>
    public LayoutElement CreateLayout()
    {
        _help.Text = HelpText ?? "";
        var extraButtons = L.Row(
            CreateExtraButtons(),
            HelpText == null ? C.None() : _help.Button);
        var okCancel = L.Row(L.OkCancel(OkButton, CancelButton));
        return L.Column(
            CreateControls(),
            HelpText == null ? C.None() : _help.Label,
            L.Adaptive(
                L.Row(extraButtons, C.Filler(), okCancel),
                L.Column(L.Row(extraButtons, C.Filler()), L.Row(C.Filler(), okCancel))));
    }

    public void OnShown()
    {
        SetDefaultFocus();
    }

    public void HandleKeyDown(KeyEventArgs e) => OnKeyDown(e);

    protected virtual void OnKeyDown(KeyEventArgs e)
    {
    }

    protected virtual void SetDefaultFocus()
    {
        OkButton.Focus();
    }

    protected void AttachDpiDependency(Action<float> callback) =>
        EtoPlatform.Current.AttachDpiDependency(OkButton, callback);

    private void ViewerMouseDown(object? sender, MouseEventArgs e) => OnMouseDown(e);
    private void ViewerMouseMove(object? sender, MouseEventArgs e) => OnMouseMove(e);
    private void ViewerMouseUp(object? sender, MouseEventArgs e) => OnMouseUp(e);

    protected virtual void OnMouseDown(MouseEventArgs e)
    {
    }

    protected virtual void OnMouseMove(MouseEventArgs e)
    {
    }

    protected virtual void OnMouseUp(MouseEventArgs e)
    {
    }

    protected virtual void PaintOverlay(object? sender, PaintEventArgs e)
    {
    }

    protected void Invalidate() => _viewer?.Invalidate();

    private void RenderImage()
    {
        var bitmap = RenderPreview();
        Invoker.Current.Invoke(() =>
        {
            DisplayImage?.Dispose();
            DisplayImage = bitmap;
            ShowDisplayImage();
        });
    }

    private void ShowDisplayImage()
    {
        if (DisplayImage == null || _viewer == null) return;
        var old = _viewerBitmap;
        _viewerBitmap = DisplayImage.ToEtoImage();
        _viewer.SetImage(_viewerBitmap);
        old?.Dispose();
    }

    protected abstract LayoutElement CreateControls();

    protected virtual LayoutElement CreateExtraButtons() => C.None();

    protected abstract IMemoryImage RenderPreview();

    protected abstract void InitDisplayImage();

    protected abstract void Apply();

    protected void UpdatePreviewBox()
    {
        Invalidate();
        _renderThrottle.RunAction();
    }

    protected RectangleF GetScreenWorkingArea()
    {
        try
        {
            var screen = ParentWindow?.Screen ?? Screen.PrimaryScreen;
            if (screen != null)
            {
                return screen.WorkingArea;
            }
        }
        catch (Exception)
        {
            // On Linux sometimes we can't get the working area
        }

        // Assume 1080p screen by default
        return new RectangleF(0, 0, 1920, 1080);
    }

    public virtual void Dispose()
    {
        if (_viewer != null)
        {
            _viewer.PaintOverlay -= PaintOverlay;
            _viewer.OverlayMouseDown -= ViewerMouseDown;
            _viewer.OverlayMouseMove -= ViewerMouseMove;
            _viewer.OverlayMouseUp -= ViewerMouseUp;
            _viewer.PanEnabled = true;
            _viewer.FitIsMinZoom = false;
            _viewer.ImagePadding = 0;
            _viewer.OverlayCursor = null;
            if (_viewer.Image == _viewerBitmap)
            {
                _viewer.SetImage(null);
            }
            _viewer = null;
        }
        _viewerBitmap?.Dispose();
        _viewerBitmap = null;
        DisplayImage?.Dispose();
        DisplayImage = null;
    }
}
