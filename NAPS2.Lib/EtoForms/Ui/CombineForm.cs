using Eto.Drawing;
using NAPS2.EtoForms.Layout;
using NAPS2.Scan;

namespace NAPS2.EtoForms.Ui;

public class CombineForm : ImageFormBase
{
    private readonly IIconProvider _iconProvider;
    private readonly ScanningContext _scanningContext;

    private readonly LayoutVisibility _hVis = new(false);
    private readonly LayoutVisibility _vVis = new(false);
    private readonly LayoutVisibility _hAlignVis = new(true);
    private readonly LayoutVisibility _vAlignVis = new(true);
    private CombineOrientation _orientation;
    private double _hOffset = 0.5;
    private double _vOffset = 0.5;

    public CombineForm(Naps2Config config, UiImageList imageList, ThumbnailController thumbnailController,
        IIconProvider iconProvider, ScanningContext scanningContext) :
        base(config, imageList, thumbnailController)
    {
        _iconProvider = iconProvider;
        _scanningContext = scanningContext;
        Icon = new Icon(1f, Icons.combine.ToEtoImage());
        Title = UiStrings.Combine;
        CanApplyToAllSelected = false;
        ShowRevertButton = false;
    }

    private UiImage Image1 { get; set; } = null!;
    private UiImage Image2 { get; set; } = null!;

    private IMemoryImage WorkingImage1 { get; set; } = null!;
    private IMemoryImage WorkingImage2 { get; set; } = null!;

    protected override LayoutElement CreateControls()
    {
        // TODO: Why is there a form size change when we first toggle orientation?
        return L.Row(
            C.Filler(),
            L.Row(
                L.Row(
                    C.IconButton(_iconProvider.GetIcon("shape_align_left")!, () => SetHOffset(0)),
                    C.IconButton(_iconProvider.GetIcon("shape_align_center")!, () => SetHOffset(0.5)),
                    C.IconButton(_iconProvider.GetIcon("shape_align_right")!, () => SetHOffset(1.0))
                ).Visible(_hAlignVis),
                C.IconButton(_iconProvider.GetIcon("combine_hor")!, () => SetOrientation(CombineOrientation.Horizontal))
                    .Padding(left: 20),
                C.IconButton(_iconProvider.GetIcon("switch")!, SwapImages)
            ).Visible(_vVis),
            L.Row(
                L.Row(
                    C.IconButton(_iconProvider.GetIcon("shape_align_top")!, () => SetVOffset(0)),
                    C.IconButton(_iconProvider.GetIcon("shape_align_middle")!, () => SetVOffset(0.5)),
                    C.IconButton(_iconProvider.GetIcon("shape_align_bottom")!, () => SetVOffset(1.0))
                ).Visible(_vAlignVis),
                C.IconButton(_iconProvider.GetIcon("combine")!, () => SetOrientation(CombineOrientation.Vertical))
                    .Padding(left: 20),
                C.IconButton(_iconProvider.GetIcon("switch_hor")!, SwapImages)
            ).Visible(_hVis),
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
        _hVis.IsVisible = _orientation == CombineOrientation.Horizontal;
        _vVis.IsVisible = _orientation == CombineOrientation.Vertical;
        UpdatePreviewBox();
    }

    protected override void InitDisplayImage()
    {
        // If there's an image after this one, then this is the first image, and the subsequent image is the second.
        // Otherwise, we look for the previous image in the list, which should be considered the first image, and then
        // this image is the second.
        var nextImage = SelectedImages?.ElementAtOrDefault(1) ??
                        _imageList.Images.ElementAtOrDefault(_imageList.Images.IndexOf(Image) + 1);
        Image1 = nextImage != null
            ? Image
            : _imageList.Images.ElementAtOrDefault(_imageList.Images.IndexOf(Image) - 1) ??
              throw new InvalidOperationException("No image to combine with");
        Image2 = nextImage ?? Image;

        using var processedImage1 = Image1.GetClonedImage();
        WorkingImage1 = processedImage1.Render();
        using var processedImage2 = Image2.GetClonedImage();
        WorkingImage2 = processedImage2.Render();

        _orientation = WorkingImage1.Width + WorkingImage2.Width > WorkingImage1.Height + WorkingImage2.Height
            ? CombineOrientation.Vertical
            : CombineOrientation.Horizontal;
        _hVis.IsVisible = _orientation == CombineOrientation.Horizontal;
        _vVis.IsVisible = _orientation == CombineOrientation.Vertical;
        // We could make these visibilities different (i.e. hAlignVis is only based on if widths are different), but
        // that means the button alignment can change underneath the mouse which isn't great.
        _hAlignVis.IsVisible = WorkingImage1.Width != WorkingImage2.Width || WorkingImage1.Height != WorkingImage2.Height;
        _vAlignVis.IsVisible = WorkingImage1.Width != WorkingImage2.Width || WorkingImage1.Height != WorkingImage2.Height;

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
        var thumbnail = combinedImage.Clone().PerformTransform(new ThumbnailTransform(_thumbnailController.RenderSize));
        var ppd = new PostProcessingData { Thumbnail = thumbnail, ThumbnailTransformState = TransformState.Empty };
        var processedImage = _scanningContext.CreateProcessedImage(combinedImage).WithPostProcessingData(ppd, true);

        _imageList.Mutate(new ListMutation<UiImage>.InsertAfter(new UiImage(processedImage), Image));
        // TODO: Maybe have a checkbox to keep the original images?
        _imageList.Mutate(new ListMutation<UiImage>.DeleteSelected(), ListSelection.Of(Image1, Image2));
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        WorkingImage1.Dispose();
        WorkingImage2.Dispose();
    }
}