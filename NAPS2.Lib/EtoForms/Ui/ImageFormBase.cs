using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms.Layout;

namespace NAPS2.EtoForms.Ui;

public abstract class ImageFormBase : EtoDialogBase
{
    private readonly ImageView _imageView = new();

    private readonly RefreshThrottle _renderThrottle;

    // Image bounds in the coordinate space of the overlay control
    protected float _overlayT, _overlayL, _overlayR, _overlayB, _overlayW, _overlayH;

    protected ImageFormBase(Naps2Config config, UiImageList imageList, ThumbnailController thumbnailController) :
        base(config)
    {
        ImageList = imageList;
        ThumbnailController = thumbnailController;
        _renderThrottle = new RefreshThrottle(RenderImage);
        Overlay.Paint += PaintOverlay;
        Overlay.SizeChanged += (_, _) => UpdateImageCoords();
        FormStateController.DefaultExtraLayoutSize = new Size(400, 400);
    }

    public UiImage Image { get; set; } = null!;
    public List<UiImage>? SelectedImages { get; set; }

    protected UiImageList ImageList { get; }
    protected ThumbnailController ThumbnailController { get; }

    protected int DisplayImageHeight { get; set; }
    protected int DisplayImageWidth { get; set; }

    protected IMemoryImage? DisplayImage { get; set; }
    protected Drawable Overlay { get; } = new();
    protected int OverlayBorderSize { get; set; }

    protected override void BuildLayout()
    {
        LayoutController.Content = L.Column(
            Overlay.Scale(),
            CreateControls(),
            L.Row(
                CreateExtraButtons(),
                C.Filler(),
                L.OkCancel(
                    C.OkButton(this, beforeClose: Apply),
                    C.CancelButton(this))
            )
        );
    }

    protected override void OnPreLoad(EventArgs e)
    {
        base.OnPreLoad(e);
        InitDisplayImage();
        DisplayImageWidth = DisplayImage!.Width;
        DisplayImageHeight = DisplayImage.Height;
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        UpdatePreviewBox();
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        DisplayImage?.Dispose();
        _imageView.Image?.Dispose();
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        UpdateImageCoords();
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        UpdateImageCoords();
    }

    private void UpdateImageCoords()
    {
        if (!Overlay.Loaded) return;
        var widthRatio = DisplayImageWidth / (float) (Overlay.Width - OverlayBorderSize * 2);
        var heightRatio = DisplayImageHeight / (float) (Overlay.Height - OverlayBorderSize * 2);
        var ratio = widthRatio / heightRatio;
        if (ratio > 1)
        {
            _overlayL = OverlayBorderSize;
            _overlayR = Overlay.Width - OverlayBorderSize * 2;
            var empty = Overlay.Height - Overlay.Height / ratio;
            _overlayT = empty / 2 + OverlayBorderSize;
            _overlayB = Overlay.Height - empty / 2 - OverlayBorderSize * 2;
        }
        else
        {
            _overlayT = OverlayBorderSize;
            _overlayB = Overlay.Height - OverlayBorderSize * 2;
            var empty = Overlay.Width - Overlay.Width * ratio;
            _overlayL = empty / 2 + OverlayBorderSize;
            _overlayR = Overlay.Width - empty / 2 - OverlayBorderSize * 2;
        }
        _overlayW = _overlayR - _overlayL;
        _overlayH = _overlayB - _overlayT;
        Overlay.Invalidate();
    }

    protected virtual void PaintOverlay(object? sender, PaintEventArgs e)
    {
        using var etoImage = DisplayImage!.ToEtoImage();
        e.Graphics.DrawImage(etoImage, _overlayL, _overlayT, _overlayW, _overlayH);
    }

    private void RenderImage()
    {
        var bitmap = RenderPreview();
        Invoker.Current.Invoke(() =>
        {
            DisplayImage?.Dispose();
            DisplayImage = bitmap;
            if (DisplayImage.Width != DisplayImageWidth || DisplayImage.Height != DisplayImageHeight)
            {
                DisplayImageWidth = DisplayImage.Width;
                DisplayImageHeight = DisplayImage.Height;
                UpdateImageCoords();
            }
            Overlay.Invalidate();
        });
    }

    protected abstract LayoutElement CreateControls();

    protected virtual LayoutElement CreateExtraButtons() => C.None();

    protected abstract IMemoryImage RenderPreview();

    protected abstract void InitDisplayImage();

    protected abstract void Apply();

    protected void UpdatePreviewBox()
    {
        Overlay.Invalidate();
        _renderThrottle.RunAction();
    }

    protected RectangleF GetScreenWorkingArea()
    {
        try
        {
            var screen = Screen ?? Screen.PrimaryScreen;
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
}