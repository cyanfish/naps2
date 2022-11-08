namespace NAPS2.EtoForms.Ui;

public class HueSatForm : ImageFormBase
{
    private readonly SliderWithTextBox _hueSlider = new();
    private readonly SliderWithTextBox _saturationSlider = new();

    public HueSatForm(Naps2Config config, ThumbnailController thumbnailController, IIconProvider iconProvider) :
        base(config, thumbnailController)
    {
        _hueSlider.Icon = iconProvider.GetIcon("color_wheel");
        _saturationSlider.Icon = iconProvider.GetIcon("color_gradient");
        Sliders = new[] { _hueSlider, _saturationSlider };
    }

    protected override IEnumerable<Transform> Transforms =>
        new Transform[]
        {
            new HueTransform(_hueSlider.Value),
            new SaturationTransform(_saturationSlider.Value)
        };
}