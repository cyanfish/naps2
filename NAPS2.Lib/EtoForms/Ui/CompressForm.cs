using Eto.Forms;
using NAPS2.EtoForms.Layout;
using NAPS2.EtoForms.Widgets;
using NAPS2.Scan;

namespace NAPS2.EtoForms.Ui;

/// <summary>
/// Reduces (or otherwise changes) an image's resolution and color depth.
/// </summary>
public class CompressForm : UnaryImageFormBase
{
    private const float DEFAULT_DPI = 96;

    private readonly UndoableNumericMaskedTextBox<decimal> _dpi = new();
    private readonly UndoableNumericMaskedTextBox<decimal> _width = new();
    private readonly UndoableNumericMaskedTextBox<decimal> _height = new();
    private readonly EnumDropDownWidget<LocalizedPageSizeUnit> _unit = new();
    private readonly UndoableNumericMaskedTextBox<int> _pixelWidth = new();
    private readonly UndoableNumericMaskedTextBox<int> _pixelHeight = new();
    private readonly EnumDropDownWidget<ScanBitDepth> _bitDepth = new();
    private readonly CheckBox _useJpegCompression = C.CheckBox(UiStrings.UseJpegCompression);
    private readonly UndoableNumericMaskedTextBox<int> _jpegQuality = new();
    private readonly LayoutVisibility _jpegQualityVis = new(false);

    private float _originalDpi;
    private ScanBitDepth _originalBitDepth;
    private int _originalJpegQuality;

    private double _dpiValue;
    private PageSize _size = new(0, 0, PageSizeUnit.Inch);
    private ScanBitDepth _bitDepthValue;
    private bool _updatingFields;

    public CompressForm(Naps2Config config, UiImageList imageList, ThumbnailController thumbnailController) :
        base(config, imageList, thumbnailController)
    {
        IconName = "spring_small";
        Title = UiStrings.Compress;

        _dpi.TextChanged += (_, _) => DpiChanged();
        _width.TextChanged += (_, _) => PhysicalSizeChanged(_width, isWidth: true);
        _height.TextChanged += (_, _) => PhysicalSizeChanged(_height, isWidth: false);
        _unit.SelectedItemChanged += (_, _) => UpdateFields();
        _pixelWidth.TextChanged += (_, _) => PixelSizeChanged(_pixelWidth, isWidth: true);
        _pixelHeight.TextChanged += (_, _) => PixelSizeChanged(_pixelHeight, isWidth: false);
        _bitDepth.SelectedItemChanged += (_, _) =>
        {
            _bitDepthValue = _bitDepth.SelectedItem;
            RefreshPreview();
        };
        _useJpegCompression.CheckedChanged += (_, _) => UpdateJpegQualityVisibility();
    }

    private void UpdateJpegQualityVisibility() =>
        _jpegQualityVis.IsVisible = !Image.Metadata.Lossless || _useJpegCompression.IsChecked();

    private int JpegQuality => _jpegQuality.Value is >= 1 and <= 100 ? _jpegQuality.Value : _originalJpegQuality;

    private bool JpegQualityChanged => _jpegQualityVis.IsVisible && JpegQuality != _originalJpegQuality;

    private int TargetPixelWidth => Math.Max((int) Math.Round((double) _size.WidthInInches * _dpiValue), 1);

    private int TargetPixelHeight => Math.Max((int) Math.Round((double) _size.HeightInInches * _dpiValue), 1);

    protected override List<Transform> Transforms
    {
        get
        {
            var transforms = new List<Transform>();
            if (TargetPixelWidth != RealImageWidth || TargetPixelHeight != RealImageHeight)
            {
                transforms.Add(new ScaleTransform(TargetPixelWidth / (double) RealImageWidth));
                // Resampling may also change the resolution, so always set it explicitly afterward
                transforms.Add(new ResolutionTransform((float) _dpiValue));
            }
            else if (Math.Abs(_dpiValue - _originalDpi) > 0.001)
            {
                transforms.Add(new ResolutionTransform((float) _dpiValue));
            }
            if (_bitDepthValue != _originalBitDepth)
            {
                transforms.Add(_bitDepthValue switch
                {
                    ScanBitDepth.BlackWhite => new BlackWhiteTransform(),
                    ScanBitDepth.Grayscale => new GrayscaleTransform(),
                    _ => new ColorBitDepthTransform()
                });
            }
            if (_useJpegCompression.IsChecked() || JpegQualityChanged)
            {
                // This also ensures an untransformed JPEG file is re-encoded (rather than copied) when the quality
                // changes
                transforms.Add(new FileFormatTransform(ImageFileFormat.Jpeg));
            }
            return transforms;
        }
    }

    protected override void OnFullSizeImageRendered(IMemoryImage realImage)
    {
        _originalDpi = realImage.HorizontalResolution > 0 ? realImage.HorizontalResolution : DEFAULT_DPI;
        _originalJpegQuality = Image.Metadata.Quality > 0
            ? Image.Metadata.Quality
            : ScanOptions.DEFAULT_QUALITY;
        _originalBitDepth = realImage.UpdateLogicalPixelFormat() switch
        {
            ImagePixelFormat.BW1 => ScanBitDepth.BlackWhite,
            ImagePixelFormat.Gray8 => ScanBitDepth.Grayscale,
            _ => ScanBitDepth.C24Bit
        };
        Revert();
        // If the image is a well-known paper size (in either orientation), show the size in that paper's unit (e.g. mm
        // for A4), otherwise use the unit of the system's default paper size
        var unitSize = PageSize.FindWellKnownSize(_size.Width, _size.Height, _size.Unit) ??
                       PageSize.FindWellKnownSize(_size.Height, _size.Width, _size.Unit) ??
                       PageSize.SystemDefault;
        _unit.SelectedItem = (LocalizedPageSizeUnit) unitSize.Unit;
        UpdateFields();
    }

    protected override LayoutElement CreateControls()
    {
        return L.Column(
            L.Row(
                C.Label(UiStrings.ResolutionDpiLabel).AlignCenter(),
                L.Row(
                    _dpi.Width(60).AlignCenter(),
                    C.Filler()
                ).Scale()
            ).Aligned(),
            L.Row(
                C.Label(UiStrings.DimensionsLabel).AlignCenter(),
                L.Row(
                    _width.Width(60).AlignCenter(),
                    C.Label("X").AlignCenter(),
                    _height.Width(60).AlignCenter(),
                    _unit.AsControl().AlignCenter(),
                    C.Filler()
                ).Scale()
            ).Aligned(),
            L.Row(
                C.Label(UiStrings.PixelDimensionsLabel).AlignCenter(),
                L.Row(
                    _pixelWidth.Width(60).AlignCenter(),
                    C.Label("X").AlignCenter(),
                    _pixelHeight.Width(60).AlignCenter(),
                    C.Filler()
                ).Scale()
            ).Aligned(),
            L.Row(
                C.Label(UiStrings.BitDepthLabel).AlignCenter(),
                L.Row(
                    _bitDepth.AsControl().AlignCenter(),
                    C.Filler()
                ).Scale()
            ).Aligned(),
            // Only lossless images (e.g. imported from PNG) can be converted
            Image.Metadata.Lossless ? _useJpegCompression : C.None(),
            L.Row(
                C.Label(UiStrings.JpegQualityLabel).AlignCenter(),
                L.Row(
                    _jpegQuality.Width(60).AlignCenter(),
                    C.Filler()
                ).Scale()
            ).Aligned().Visible(_jpegQualityVis),
            ApplyToSelectedControl
        );
    }

    protected override void Revert()
    {
        _dpiValue = _originalDpi;
        _size = new PageSize((decimal) (RealImageWidth / _dpiValue), (decimal) (RealImageHeight / _dpiValue),
            PageSizeUnit.Inch);
        _bitDepth.SelectedItem = _bitDepthValue = _originalBitDepth;
        _useJpegCompression.Checked = false;
        _jpegQuality.Value = _originalJpegQuality;
        UpdateJpegQualityVisibility();
        UpdateFields();
    }

    protected override ListMutation<UiImage> CreateMutation()
    {
        var mutation = base.CreateMutation();
        bool useJpegCompression = _useJpegCompression.IsChecked();
        if (useJpegCompression || JpegQualityChanged)
        {
            int quality = JpegQuality;
            return new CompoundListMutation<UiImage>(
                new ImageListMutation.UpdateMetadata(metadata => metadata with
                {
                    Lossless = metadata.Lossless && !useJpegCompression,
                    Quality = quality
                }),
                mutation);
        }
        return mutation;
    }

    // Changing the resolution keeps the physical size, so the image is resampled
    private void DpiChanged()
    {
        // Empty or invalid text has a value of 0
        if (_updatingFields || _dpi.Value <= 0) return;
        _dpiValue = (double) _dpi.Value;
        UpdateFields(skip: _dpi);
    }

    // Changing the physical size keeps the pixels, so only the resolution changes
    private void PhysicalSizeChanged(NumericMaskedTextBox<decimal> textBox, bool isWidth)
    {
        if (_updatingFields || textBox.Value <= 0) return;
        int pixelWidth = TargetPixelWidth;
        // Keep the aspect ratio
        decimal value = textBox.Value;
        decimal aspectRatio = RealImageHeight / (decimal) RealImageWidth;
        var unit = (PageSizeUnit) _unit.SelectedItem;
        _size = isWidth
            ? new PageSize(value, value * aspectRatio, unit)
            : new PageSize(value / aspectRatio, value, unit);
        _dpiValue = pixelWidth / (double) _size.WidthInInches;
        UpdateFields(skip: textBox);
    }

    // Changing the pixel size keeps the physical size, so the image is resampled at a new resolution
    private void PixelSizeChanged(NumericMaskedTextBox<int> textBox, bool isWidth)
    {
        if (_updatingFields || textBox.Value <= 0) return;
        _dpiValue = textBox.Value / (double) (isWidth ? _size.WidthInInches : _size.HeightInInches);
        UpdateFields(skip: textBox);
    }

    private void UpdateFields(TextControl? skip = null)
    {
        _updatingFields = true;
        try
        {
            if (skip != _dpi) _dpi.Value = (decimal) Math.Round(_dpiValue);
            var displaySize = _size.ConvertTo((PageSizeUnit) _unit.SelectedItem);
            int decimals = displaySize.Unit == PageSizeUnit.Millimetre ? 1 : 2;
            // Round as a double so there are no trailing zeros (e.g. 8.5 rather than 8.50)
            if (skip != _width) _width.Value = (decimal) Math.Round((double) displaySize.Width, decimals);
            if (skip != _height) _height.Value = (decimal) Math.Round((double) displaySize.Height, decimals);
            if (skip != _pixelWidth) _pixelWidth.Value = TargetPixelWidth;
            if (skip != _pixelHeight) _pixelHeight.Value = TargetPixelHeight;
        }
        finally
        {
            _updatingFields = false;
        }
        RefreshPreview();
    }

    private void RefreshPreview()
    {
        if (Loaded)
        {
            UpdatePreviewBox();
        }
    }
}