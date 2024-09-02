using Eto.Drawing;
using NAPS2.EtoForms.Layout;
using NAPS2.Scan;

namespace NAPS2.EtoForms.Ui;

public class CombineForm : ImageFormBase
{
    private readonly ScanningContext _scanningContext;

    private readonly LayoutVisibility _horizontalOrientationVis = new(false);
    private readonly LayoutVisibility _alignVis = new(true);
    private CombineOrientation _orientation;
    private double _hOffset = 0.5;
    private double _vOffset = 0.5;

    public CombineForm(Naps2Config config, UiImageList imageList, ThumbnailController thumbnailController,
        ScanningContext scanningContext) :
        base(config, imageList, thumbnailController)
    {
        Title = UiStrings.Combine;
        IconName = "combine_small";

        _scanningContext = scanningContext;
    }

    private UiImage Image1 { get; set; } = null!;
    private UiImage Image2 { get; set; } = null!;

    private IMemoryImage WorkingImage1 { get; set; } = null!;
    private IMemoryImage WorkingImage2 { get; set; } = null!;

    protected override LayoutElement CreateControls()
    {
        return L.Row(
            C.Filler(),
            L.Row(
                L.Row(
                    C.IconButton("shape_align_left_small", () => SetHOffset(0)),
                    C.IconButton("shape_align_center_small", () => SetHOffset(0.5)),
                    C.IconButton("shape_align_right_small", () => SetHOffset(1.0))
                ).Visible(_alignVis),
                C.IconButton("combine_hor_small",
                        () => SetOrientation(CombineOrientation.Horizontal))
                    .Padding(left: 20),
                C.IconButton("switch_ver_small", SwapImages)
            ).Visible(!_horizontalOrientationVis),
            L.Row(
                L.Row(
                    C.IconButton("shape_align_top_small", () => SetVOffset(0)),
                    C.IconButton("shape_align_middle_small", () => SetVOffset(0.5)),
                    C.IconButton("shape_align_bottom_small", () => SetVOffset(1.0))
                ).Visible(_alignVis),
                C.IconButton("combine_ver_small",
                        () => SetOrientation(CombineOrientation.Vertical))
                    .Padding(left: 20),
                C.IconButton("switch_hor_small", SwapImages)
            ).Visible(_horizontalOrientationVis),
            C.Filler()
        );
    }

    private void SwapImages()
    {
        (Image1, Image2) = (Image2, Image1);
        (WorkingImage1, WorkingImage2) = (WorkingImage2, WorkingImage1);
        UpdatePreviewBox();
    }

    private void SetHOffset(double value)
    {
        _hOffset = value;
        UpdatePreviewBox();
    }

    private void SetVOffset(double value)
    {
        _vOffset = value;
        UpdatePreviewBox();
    }

    private void SetOrientation(CombineOrientation orientation)
    {
        _orientation = orientation;
        _horizontalOrientationVis.IsVisible = _orientation == CombineOrientation.Horizontal;
        UpdatePreviewBox();
    }

    protected override void InitDisplayImage()
    {
        // If there's an image after this one, then this is the first image, and the subsequent image is the second.
        // Otherwise, we look for the previous image in the list, which should be considered the first image, and then
        // this image is the second.
        var nextImage = SelectedImages?.ElementAtOrDefault(1) ??
                        ImageList.Images.ElementAtOrDefault(ImageList.Images.IndexOf(Image) + 1);
        Image1 = nextImage != null
            ? Image
            : ImageList.Images.ElementAtOrDefault(ImageList.Images.IndexOf(Image) - 1) ??
              throw new InvalidOperationException("No image to combine with");
        Image2 = nextImage ?? Image;

        using var processedImage1 = Image1.GetClonedImage();
        WorkingImage1 = processedImage1.Render();
        using var processedImage2 = Image2.GetClonedImage();
        WorkingImage2 = processedImage2.Render();

        _orientation = WorkingImage1.Width + WorkingImage2.Width > WorkingImage1.Height + WorkingImage2.Height
            ? CombineOrientation.Vertical
            : CombineOrientation.Horizontal;
        _horizontalOrientationVis.IsVisible = _orientation == CombineOrientation.Horizontal;
        // We could make these visibilities different (i.e. hAlignVis + vAlignVis), but
        // that means the button alignment can change underneath the mouse which isn't great.
        _alignVis.IsVisible =
            WorkingImage1.Width != WorkingImage2.Width || WorkingImage1.Height != WorkingImage2.Height;

        var workingArea = GetScreenWorkingArea();
        var widthRatio1 = WorkingImage1.Width / workingArea.Width;
        var heightRatio1 = WorkingImage1.Height / workingArea.Height;
        var widthRatio2 = WorkingImage1.Width / workingArea.Width;
        var heightRatio2 = WorkingImage1.Height / workingArea.Height;
        var maxRatio = new[] { widthRatio1, heightRatio1, widthRatio2, heightRatio2 }.Max();
        if (maxRatio > 1)
        {
            WorkingImage1 = WorkingImage1.PerformTransform(new ScaleTransform(1 / maxRatio));
            WorkingImage2 = WorkingImage2.PerformTransform(new ScaleTransform(1 / maxRatio));
        }

        // TODO: We probably want to scale up any lower-res images to match the higher resolution. Here and in Apply().

        DisplayImage = RenderPreview();
    }

    protected override IMemoryImage RenderPreview()
    {
        return CombineImages(WorkingImage1, WorkingImage2);
    }

    private IMemoryImage CombineImages(IMemoryImage first, IMemoryImage second)
    {
        return MoreImageTransforms.Combine(first, second, _orientation,
            _orientation == CombineOrientation.Horizontal ? _vOffset : _hOffset);
    }

    protected override void Apply()
    {
        // TODO: Consider the latency of this, especially with "Apply all". Does it make sense to be async? Have a operation?
        using var processedImage1 = Image1.GetClonedImage();
        using var renderedImage1 = processedImage1.Render();
        using var processedImage2 = Image2.GetClonedImage();
        using var renderedImage2 = processedImage2.Render();
        using var combinedImage = CombineImages(renderedImage1, renderedImage2);

        // TODO: Use working images for thumbnail?
        var thumbnail = combinedImage.Clone().PerformTransform(new ThumbnailTransform(ThumbnailController.RenderSize));
        var ppd = new PostProcessingData { Thumbnail = thumbnail, ThumbnailTransformState = TransformState.Empty };
        var processedImage = _scanningContext.CreateProcessedImage(combinedImage).WithPostProcessingData(ppd, true);

        ImageList.Mutate(new ListMutation<UiImage>.InsertAfter(new UiImage(processedImage), Image));
        // TODO: Maybe have a checkbox to keep the original images?
        ImageList.Mutate(new ListMutation<UiImage>.DeleteSelected(), ListSelection.Of(Image1, Image2));
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        WorkingImage1.Dispose();
        WorkingImage2.Dispose();
    }
}