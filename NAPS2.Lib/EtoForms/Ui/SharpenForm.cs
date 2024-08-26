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
        Icon = iconProvider.GetFormIcon("sharpen_small");
        Title = UiStrings.Sharpen;

        _sharpenSlider.Icon = iconProvider.GetIcon("sharpen_small");
        Sliders = [_sharpenSlider];
    }

    protected override List<Transform> Transforms => [new SharpenTransform(_sharpenSlider.IntValue)];
}