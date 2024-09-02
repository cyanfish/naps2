using Eto.Drawing;
using NAPS2.EtoForms.Widgets;

namespace NAPS2.EtoForms.Ui;

public class SharpenForm : UnaryImageFormBase
{
    private readonly SliderWithTextBox _sharpenSlider = new();

    public SharpenForm(Naps2Config config, UiImageList imageList, ThumbnailController thumbnailController,
        IIconProvider iconProvider) :
        base(config, imageList, thumbnailController)
    {
        IconName = "sharpen_small";
        Title = UiStrings.Sharpen;

        EtoPlatform.Current.AttachDpiDependency(this,
            scale => _sharpenSlider.Icon = iconProvider.GetIcon("sharpen_small", scale));
        Sliders = [_sharpenSlider];
    }

    protected override List<Transform> Transforms => [new SharpenTransform(_sharpenSlider.IntValue)];
}