using NAPS2.EtoForms.Widgets;

namespace NAPS2.EtoForms.Ui;

public class BlackWhiteForm : UnaryImageFormBase
{
    private readonly SliderWithTextBox _thresholdSlider = new();

    public BlackWhiteForm(Naps2Config config, UiImageList imageList, ThumbnailController thumbnailController,
        IIconProvider iconProvider) :
        base(config, imageList, thumbnailController)
    {
        IconName = "contrast_high_small";
        Title = UiStrings.BlackAndWhite;

        EtoPlatform.Current.AttachDpiDependency(this,
            scale => _thresholdSlider.Icon = iconProvider.GetIcon("contrast_high_small", scale));
        Sliders = [_thresholdSlider];
        // BlackWhiteTransform is not commutative with scaling
        CanScaleWorkingImage = false;
    }

    protected override List<Transform> Transforms => [new BlackWhiteTransform(_thresholdSlider.IntValue)];
}