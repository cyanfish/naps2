namespace NAPS2.EtoForms.Ui;

public class BlackWhiteForm : ImageFormBase
{
    private readonly SliderWithTextBox _thresholdSlider = new();

    public BlackWhiteForm(Naps2Config config, ThumbnailController thumbnailController, IIconProvider iconProvider) :
        base(config, thumbnailController)
    {
        _thresholdSlider.Icon = iconProvider.GetIcon("contrast_high");
        Sliders = new[] { _thresholdSlider };
    }

    protected override IEnumerable<Transform> Transforms =>
        new Transform[]
        {
            new BlackWhiteTransform(_thresholdSlider.Value)
        };
}