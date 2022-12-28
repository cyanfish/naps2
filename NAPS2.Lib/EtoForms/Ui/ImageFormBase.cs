using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms.Layout;
using NAPS2.EtoForms.Widgets;

namespace NAPS2.EtoForms.Ui;

public abstract class ImageFormBase : EtoDialogBase
{
    private readonly ThumbnailController _thumbnailController;

    private readonly ImageView _imageView = new();
    private readonly CheckBox _applyToSelected = new();
    private readonly Button _revert = C.Button(UiStrings.Revert);

    private readonly RefreshThrottle _renderThrottle;

    // Image bounds in the coordinate space of the overlay control
    protected float _overlayT, _overlayL, _overlayR, _overlayB, _overlayW, _overlayH;

    public ImageFormBase(Naps2Config config, ThumbnailController thumbnailController) : base(config)
    {
        _thumbnailController = thumbnailController;
        _revert.Click += Revert;
        _renderThrottle = new RefreshThrottle(RenderImage);
        Overlay.Paint += PaintOverlay;
        Overlay.SizeChanged += (_, _) => UpdateImageCoords();
        FormStateController.DefaultExtraLayoutSize = new Size(400, 400);
    }

    protected override void BuildLayout()
    {
        foreach (var slider in Sliders)
        {
            slider.ValueChanged += UpdatePreviewBox;
        }

        LayoutController.Content = L.Column(
            Overlay.Scale(),
            CreateControls(),
            SelectedImages is { Count: > 1 } ? _applyToSelected : C.None(),
            L.Row(
                _revert,
                C.Filler(),
                L.OkCancel(
                    C.OkButton(this, beforeClose: Apply),
                    C.CancelButton(this))
            )
        );
    }

    protected int ImageHeight { get; set; }
    protected int ImageWidth { get; set; }

    protected IMemoryImage? WorkingImage { get; private set; }
    protected IMemoryImage? DisplayImage { get; private set; }
    protected Drawable Overlay { get; } = new();
    protected int OverlayBorderSize { get; set; }

    protected SliderWithTextBox[] Sliders { get; set; } = Array.Empty<SliderWithTextBox>();

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
        var widthRatio = ImageWidth / (float) (Overlay.Width - OverlayBorderSize * 2);
        var heightRatio = ImageHeight / (float) (Overlay.Height - OverlayBorderSize * 2);
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
        e.Graphics.DrawImage(DisplayImage!.ToEtoImage(), _overlayL, _overlayT, _overlayW, _overlayH);
    }

    private void RenderImage()
    {
        var bitmap = RenderPreview();
        Invoker.Current.Invoke(() =>
        {
            DisplayImage?.Dispose();
            DisplayImage = bitmap;
            if (DisplayImage.Width != ImageWidth || DisplayImage.Height != ImageHeight)
            {
                ImageWidth = DisplayImage.Width;
                ImageHeight = DisplayImage.Height;
                UpdateImageCoords();
            }
            Overlay.Invalidate();
        });
    }

    protected virtual LayoutElement CreateControls()
    {
        return L.Column(Sliders.Select(x => (LayoutElement) x).ToArray());
    }

    public UiImage Image { get; set; } = null!;

    public List<UiImage>? SelectedImages { get; set; }

    protected virtual IEnumerable<Transform> Transforms => throw new NotImplementedException();

    private bool TransformMultiple => SelectedImages != null && _applyToSelected.IsChecked();

    private IEnumerable<UiImage> ImagesToTransform => TransformMultiple ? SelectedImages! : Enumerable.Repeat(Image, 1);

    protected virtual IMemoryImage RenderPreview()
    {
        var result = WorkingImage!.Clone();
        return result.PerformAllTransforms(Transforms);
    }

    protected virtual void InitTransform()
    {
    }

    protected virtual void ResetTransform()
    {
        foreach (var slider in Sliders)
        {
            slider.Value = 0;
        }
    }

    protected virtual void TransformSaved()
    {
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        _applyToSelected.Text = string.Format(UiStrings.ApplyToSelected, SelectedImages?.Count);

        using var imageToRender = Image.GetClonedImage();
        WorkingImage = imageToRender.Render();
        DisplayImage = WorkingImage.Clone();
        ImageWidth = DisplayImage.Width;
        ImageHeight = DisplayImage.Height;
        InitTransform();
        UpdatePreviewBox();
    }

    protected void UpdatePreviewBox()
    {
        Overlay.Invalidate();
        _renderThrottle.RunAction();
    }

    private void Apply()
    {
        if (Transforms.Any(x => !x.IsNull))
        {
            foreach (var img in ImagesToTransform)
            {
                IMemoryImage? updatedThumb = null;
                if (img == Image && WorkingImage != null)
                {
                    // Optimize thumbnail rendering for the first (or only) image since we already have it loaded into memory
                    var transformed = WorkingImage.Clone().PerformAllTransforms(Transforms);
                    updatedThumb =
                        transformed.PerformTransform(new ThumbnailTransform(_thumbnailController.RenderSize));
                }
                img.AddTransforms(Transforms, updatedThumb);
            }
        }
        TransformSaved();
    }

    private void Revert(object? sender, EventArgs e)
    {
        ResetTransform();
        UpdatePreviewBox();
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        WorkingImage?.Dispose();
        DisplayImage?.Dispose();
        _imageView.Image?.Dispose();
    }
}