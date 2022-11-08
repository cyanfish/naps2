using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms.Layout;

namespace NAPS2.EtoForms.Ui;

public abstract class ImageFormBase : EtoDialogBase
{
    private readonly ThumbnailController _thumbnailController;

    private readonly ImageView _imageView = new();
    private readonly CheckBox _applyToSelected = new();
    private readonly Button _revert = C.Button(UiStrings.Revert);

    private readonly RefreshThrottle _renderThrottle;
    private IMemoryImage? _workingImage;

    public ImageFormBase(Naps2Config config, ThumbnailController thumbnailController) : base(config)
    {
        _thumbnailController = thumbnailController;
        _revert.Click += Revert;
        _renderThrottle = new RefreshThrottle(RenderImage);
        FormStateController.DefaultExtraLayoutSize = new Size(400, 400);
    }

    protected Drawable Overlay { get; } = new();

    protected SliderWithTextBox[] Sliders { get; set; } = Array.Empty<SliderWithTextBox>();

    private void RenderImage()
    {
        var bitmap = RenderPreview();
        Invoker.Current.SafeInvoke(() =>
        {
            _imageView.Image?.Dispose();
            _imageView.Image = bitmap.ToEtoImage();
        });
    }

    protected virtual LayoutElement CreateControls()
    {
        return L.Column(Sliders.Select(x => (LayoutElement) x).ToArray());
    }

    public UiImage Image { get; set; }

    public List<UiImage>? SelectedImages { get; set; }

    protected virtual IEnumerable<Transform> Transforms => throw new NotImplementedException();

    private bool TransformMultiple => SelectedImages != null && _applyToSelected.IsChecked();

    private IEnumerable<UiImage> ImagesToTransform => TransformMultiple ? SelectedImages! : Enumerable.Repeat(Image, 1);

    protected virtual IMemoryImage RenderPreview()
    {
        var result = _workingImage.Clone();
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

    protected override void OnPreLoad(EventArgs e)
    {
        foreach (var slider in Sliders)
        {
            slider.ValueChanged += UpdatePreviewBox;
        }

        LayoutController.Content = L.Column(
            L.Overlay(_imageView, Overlay).YScale(),
            CreateControls(),
            SelectedImages is { Count: > 1 } ? _applyToSelected : C.None(),
            L.Row(
                _revert,
                C.Filler(),
                C.CancelButton(this),
                C.OkButton(this, beforeClose: Apply)
            )
        );

        base.OnPreLoad(e);
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        _applyToSelected.Text = string.Format(UiStrings.ApplyToSelected, SelectedImages?.Count);

        using var imageToRender = Image.GetClonedImage();
        _workingImage = imageToRender.Render();
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
                if (img == Image)
                {
                    // Optimize thumbnail rendering for the first (or only) image since we already have it loaded into memory
                    var transformed = _workingImage.Clone().PerformAllTransforms(Transforms);
                    updatedThumb =
                        transformed.PerformTransform(new ThumbnailTransform(_thumbnailController.RenderSize));
                }
                img.AddTransforms(Transforms, updatedThumb);
            }
        }
        TransformSaved();
    }

    private void Revert(object sender, EventArgs e)
    {
        ResetTransform();
        UpdatePreviewBox();
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _workingImage?.Dispose();
        _imageView.Image?.Dispose();
    }
}