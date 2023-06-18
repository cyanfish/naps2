using Eto.Drawing;
using NAPS2.EtoForms.Widgets;

namespace NAPS2.EtoForms.Ui;

public class BlackWhiteForm : ImageFormBase
{
    private readonly SliderWithTextBox _thresholdSlider = new();

    public BlackWhiteForm(Naps2Config config, ThumbnailController thumbnailController, IIconProvider iconProvider) :
        base(config, thumbnailController)
    {
        Icon = new Icon(1f, Icons.contrast_high.ToEtoImage());
        Title = UiStrings.BlackAndWhite;

        _thresholdSlider.Icon = iconProvider.GetIcon("contrast_high");
        Sliders = new[] { _thresholdSlider };
        // BlackWhiteTransform is not commutative with scaling
        CanScaleWorkingImage = false;
    }

    protected override IEnumerable<Transform> Transforms =>
        new Transform[]
        {
            new BlackWhiteTransform(_thresholdSlider.IntValue)
        };
}