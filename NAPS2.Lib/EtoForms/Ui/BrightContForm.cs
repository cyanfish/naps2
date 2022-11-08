namespace NAPS2.EtoForms.Ui;

public class BrightContForm : ImageFormBase
{
    private readonly SliderWithTextBox _brightnessSlider = new();
    private readonly SliderWithTextBox _contrastSlider = new();

    public BrightContForm(Naps2Config config, ThumbnailController thumbnailController, IIconProvider iconProvider) :
        base(config, thumbnailController)
    {
        _brightnessSlider.Icon = iconProvider.GetIcon("weather_sun");
        _contrastSlider.Icon = iconProvider.GetIcon("contrast");
        Sliders = new[] { _brightnessSlider, _contrastSlider };
    }

    protected override IEnumerable<Transform> Transforms =>
        new Transform[]
        {
            new BrightnessTransform(_brightnessSlider.Value),
            new TrueContrastTransform(_contrastSlider.Value)
        };
}