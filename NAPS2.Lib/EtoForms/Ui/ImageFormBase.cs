using System.Threading;
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

    private IMemoryImage? workingImage, workingImage2;
    private bool _closed;
    private Timer _previewTimer;
    private bool _working;
    private bool _previewOutOfDate;
    private bool _isShown;
    private bool _initComplete;

    public ImageFormBase(Naps2Config config, ThumbnailController thumbnailController) : base(config)
    {
        _thumbnailController = thumbnailController;
        _revert.Click += Revert;
        FormStateController.DefaultExtraLayoutSize = new Size(400, 400);
    }

    protected abstract LayoutElement CreateControls();

    public UiImage Image { get; set; }

    public List<UiImage>? SelectedImages { get; set; }

    protected virtual IEnumerable<Transform> Transforms => throw new NotImplementedException();

    private bool TransformMultiple => SelectedImages != null && _applyToSelected.IsChecked();

    private IEnumerable<UiImage> ImagesToTransform => TransformMultiple ? SelectedImages! : Enumerable.Repeat(Image, 1);

    protected virtual IMemoryImage RenderPreview()
    {
        var result = workingImage.Clone();
        return result.PerformAllTransforms(Transforms);
    }

    protected virtual void InitTransform()
    {
    }

    protected virtual void ResetTransform()
    {
    }

    protected virtual void TransformSaved()
    {
    }

    protected override void OnLoad(EventArgs e)
    {
        LayoutController.Content = L.Column(
            _imageView.YScale(),
            CreateControls(),
            SelectedImages is { Count: > 1 } ? _applyToSelected : null,
            L.Row(
                _revert,
                C.Filler(),
                C.CancelButton(this),
                C.OkButton(this, beforeClose: Apply)
            )
        );
        base.OnLoad(e);
        _applyToSelected.Text = string.Format(UiStrings.ApplyToSelected, SelectedImages?.Count);

        var maxDimen = Screen.Screens.Max(s => Math.Max(s.WorkingArea.Height, s.WorkingArea.Width));
        // TODO: Limit to maxDimen * 2
        using var imageToRender = Image.GetClonedImage();
        // TODO: More generic? In general how do we integrate with eto?
        workingImage = imageToRender.Render();
        if (_closed)
        {
            workingImage?.Dispose();
            return;
        }
        workingImage2 = workingImage.Clone();

        InitTransform();
        lock (this)
        {
            _initComplete = true;
        }

        UpdatePreviewBox();
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        _isShown = true;
    }

    protected void UpdatePreviewBox()
    {
        if (_previewTimer == null)
        {
            _previewTimer = new Timer(_ =>
            {
                lock (this)
                {
                    if (!_isShown || !_previewOutOfDate || _working) return;
                    _working = true;
                    _previewOutOfDate = false;
                }
                var bitmap = RenderPreview();
                Invoker.Current.SafeInvoke(() =>
                {
                    _imageView.Image?.Dispose();
                    _imageView.Image = bitmap.ToEtoImage();
                });
                lock (this)
                {
                    _working = false;
                }
            }, null, 0, 100);
        }
        lock (this)
        {
            _previewOutOfDate = true;
        }
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
                    var transformed = workingImage.Clone().PerformAllTransforms(Transforms);
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
        workingImage?.Dispose();
        workingImage2?.Dispose();
        _imageView.Image?.Dispose();
        _previewTimer?.Dispose();
        _closed = true;
    }
}