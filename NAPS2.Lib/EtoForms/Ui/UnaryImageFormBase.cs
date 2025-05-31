using Eto.Forms;
using NAPS2.EtoForms.Layout;
using NAPS2.EtoForms.Widgets;

namespace NAPS2.EtoForms.Ui;

public abstract class UnaryImageFormBase(
    Naps2Config config,
    UiImageList imageList,
    ThumbnailController thumbnailController)
    : ImageFormBase(config, imageList, thumbnailController)
{
    private readonly CheckBox _applyToSelected = new();
    private readonly Button _revert = C.Button(UiStrings.Revert);

    protected IMemoryImage? WorkingImage { get; set; }

    protected int RealImageWidth { get; private set; }

    protected int RealImageHeight { get; private set; }

    protected SliderWithTextBox[] Sliders { get; set; } = [];

    protected bool CanScaleWorkingImage { get; set; } = true;

    protected abstract List<Transform> Transforms { get; }

    protected override void OnPreLoad(EventArgs e)
    {
        _applyToSelected.Text = string.Format(UiStrings.ApplyToSelected, SelectedImages.Count);
        _applyToSelected.Checked = Config.Get(c => c.ApplyToAllSelected);
        _applyToSelected.CheckedChanged +=
            (_, _) => Config.User.Set(c => c.ApplyToAllSelected, _applyToSelected.IsChecked());
        _revert.Click += (_, _) =>
        {
            Revert();
            UpdatePreviewBox();
        };
        foreach (var slider in Sliders)
        {
            slider.ValueChanged += UpdatePreviewBox;
        }
        base.OnPreLoad(e);
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        WorkingImage?.Dispose();
    }

    protected override LayoutElement CreateControls()
    {
        return L.Column([
            ..Sliders.Select(x => (LayoutElement) x),
            ApplyToSelectedControl
        ]);
    }

    protected LayoutElement ApplyToSelectedControl => SelectedImages.Count > 1 ? _applyToSelected : C.None();

    protected override LayoutElement CreateExtraButtons() => _revert;

    private List<UiImage> ImagesToTransform => _applyToSelected.IsChecked() ? SelectedImages : [Image];

    protected override IMemoryImage RenderPreview()
    {
        var result = WorkingImage!.Clone();
        return result.PerformAllTransforms(Transforms);
    }

    protected override void InitDisplayImage()
    {
        using var imageToRender = Image.GetClonedImage();
        WorkingImage = imageToRender.Render();
        RealImageWidth = WorkingImage.Width;
        RealImageHeight = WorkingImage.Height;

        if (CanScaleWorkingImage)
        {
            // Scale down the image to the screen size for better efficiency without losing much fidelity
            var workingArea = GetScreenWorkingArea();
            var widthRatio = WorkingImage.Width / workingArea.Width;
            var heightRatio = WorkingImage.Height / workingArea.Height;
            if (widthRatio > 1 || heightRatio > 1)
            {
                WorkingImage = WorkingImage.PerformTransform(new ScaleTransform(1 / Math.Max(widthRatio, heightRatio)));
            }
        }

        DisplayImage = WorkingImage.Clone();
    }

    protected override void Apply()
    {
        IMemoryImage? firstImageThumb = null;
        if (WorkingImage != null)
        {
            // Optimize thumbnail rendering for the first (or only) image since we already have it loaded into memory
            var transformed = WorkingImage.Clone().PerformAllTransforms(Transforms);
            firstImageThumb =
                transformed.PerformTransform(new ThumbnailTransform(ThumbnailController.RenderSize));
        }
        var mutation = new ImageListMutation.AddTransforms(
            Transforms.ToList(),
            new Dictionary<UiImage, IMemoryImage?> { [Image] = firstImageThumb });
        ImageList.Mutate(mutation, ListSelection.From(ImagesToTransform));
    }

    protected virtual void Revert()
    {
        foreach (var slider in Sliders)
        {
            slider.IntValue = 0;
        }
    }
}