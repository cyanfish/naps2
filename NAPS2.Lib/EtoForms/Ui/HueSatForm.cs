using Eto.Drawing;
using NAPS2.EtoForms.Widgets;

namespace NAPS2.EtoForms.Ui;

public class HueSatForm : UnaryImageFormBase
{
    private readonly SliderWithTextBox _hueSlider = new();
    private readonly SliderWithTextBox _saturationSlider = new();

    public HueSatForm(Naps2Config config, UiImageList imageList, ThumbnailController thumbnailController,
        IIconProvider iconProvider) :
        base(config, imageList, thumbnailController)
    {
        Icon = new Icon(1f, iconProvider.GetIcon("color_management_small"));
        Title = UiStrings.HueSaturation;

        _hueSlider.Icon = iconProvider.GetIcon("color_wheel_small");
        _saturationSlider.Icon = iconProvider.GetIcon("color_gradient_small");
        Sliders = [_hueSlider, _saturationSlider];
    }

    protected override List<Transform> Transforms =>
    [
        new HueTransform(_hueSlider.IntValue),
        new SaturationTransform(_saturationSlider.IntValue)
    ];
}