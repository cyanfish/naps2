using NAPS2.EtoForms.Layout;

namespace NAPS2.EtoForms.Ui;

public class BrightContForm : ImageFormBase
{
    private readonly SliderWithTextBox _brightnessSlider = new();
    private readonly SliderWithTextBox _contrastSlider = new();

    public BrightContForm(Naps2Config config, ThumbnailController thumbnailController) : base(config,
        thumbnailController)
    {
        _brightnessSlider.ValueChanged += UpdateTransform;
        _contrastSlider.ValueChanged += UpdateTransform;
    }

    public BrightnessTransform BrightnessTransform { get; private set; } = new BrightnessTransform();

    public TrueContrastTransform TrueContrastTransform { get; private set; } = new TrueContrastTransform();

    protected override LayoutElement CreateControls()
    {
        return L.Column(
            _brightnessSlider,
            _contrastSlider
        );
    }

    protected override IEnumerable<Transform> Transforms =>
        new Transform[] { BrightnessTransform, TrueContrastTransform };

    protected override void ResetTransform()
    {
        BrightnessTransform = new BrightnessTransform();
        TrueContrastTransform = new TrueContrastTransform();
        _brightnessSlider.Value = 0;
        _contrastSlider.Value = 0;
    }

    private void UpdateTransform()
    {
        BrightnessTransform = new BrightnessTransform(_brightnessSlider.Value);
        TrueContrastTransform = new TrueContrastTransform(_contrastSlider.Value);
        UpdatePreviewBox();
    }
}