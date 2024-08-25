using Eto.Drawing;
using NAPS2.EtoForms.Widgets;

namespace NAPS2.EtoForms.Ui;

public class BlackWhiteForm : UnaryImageFormBase
{
    private readonly SliderWithTextBox _thresholdSlider = new();

    public BlackWhiteForm(Naps2Config config, UiImageList imageList, ThumbnailController thumbnailController,
        IIconProvider iconProvider) :
        base(config, imageList, thumbnailController)
    {
        Icon = new Icon(1f, iconProvider.GetIcon("contrast_high_small"));
        Title = UiStrings.BlackAndWhite;

        _thresholdSlider.Icon = iconProvider.GetIcon("contrast_high_small");
        Sliders = [_thresholdSlider];
        // BlackWhiteTransform is not commutative with scaling
        CanScaleWorkingImage = false;
    }

    protected override List<Transform> Transforms => [new BlackWhiteTransform(_thresholdSlider.IntValue)];
}