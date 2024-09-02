using NAPS2.EtoForms.Widgets;

namespace NAPS2.EtoForms.Ui;

public class BrightContForm : UnaryImageFormBase
{
    private readonly SliderWithTextBox _brightnessSlider = new();
    private readonly SliderWithTextBox _contrastSlider = new();

    public BrightContForm(Naps2Config config, UiImageList imageList, ThumbnailController thumbnailController,
        IIconProvider iconProvider) :
        base(config, imageList, thumbnailController)
    {
        IconName = "contrast_with_sun_small";
        Title = UiStrings.BrightnessContrast;

        EtoPlatform.Current.AttachDpiDependency(this, scale =>
        {
            _brightnessSlider.Icon = iconProvider.GetIcon("weather_sun_small", scale);
            _contrastSlider.Icon = iconProvider.GetIcon("contrast_small", scale);
        });
        Sliders = [_brightnessSlider, _contrastSlider];
    }

    protected override List<Transform> Transforms =>
    [
        new BrightnessTransform(_brightnessSlider.IntValue),
        new TrueContrastTransform(_contrastSlider.IntValue)
    ];
}