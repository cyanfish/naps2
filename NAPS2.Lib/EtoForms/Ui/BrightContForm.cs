using Eto.Drawing;
using NAPS2.EtoForms.Widgets;

namespace NAPS2.EtoForms.Ui;

public class BrightContForm : ImageFormBase
{
    private readonly SliderWithTextBox _brightnessSlider = new();
    private readonly SliderWithTextBox _contrastSlider = new();

    public BrightContForm(Naps2Config config, UiImageList imageList, ThumbnailController thumbnailController,
        IIconProvider iconProvider) :
        base(config, imageList, thumbnailController)
    {
        Icon = new Icon(1f, Icons.contrast_with_sun.ToEtoImage());
        Title = UiStrings.BrightnessContrast;

        _brightnessSlider.Icon = iconProvider.GetIcon("weather_sun");
        _contrastSlider.Icon = iconProvider.GetIcon("contrast");
        Sliders = [_brightnessSlider, _contrastSlider];
    }

    protected override List<Transform> Transforms =>
    [
        new BrightnessTransform(_brightnessSlider.IntValue),
        new TrueContrastTransform(_contrastSlider.IntValue)
    ];
}