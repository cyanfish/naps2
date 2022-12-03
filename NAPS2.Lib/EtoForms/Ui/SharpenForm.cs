using Eto.Drawing;

namespace NAPS2.EtoForms.Ui;

public class SharpenForm : ImageFormBase
{
    private readonly SliderWithTextBox _sharpenSlider = new();

    public SharpenForm(Naps2Config config, ThumbnailController thumbnailController, IIconProvider iconProvider) :
        base(config, thumbnailController)
    {
        Icon = new Icon(1f, Icons.sharpen.ToEtoImage());
        Title = UiStrings.Sharpen;

        _sharpenSlider.Icon = iconProvider.GetIcon("sharpen");
        Sliders = new[] { _sharpenSlider };
    }

    protected override IEnumerable<Transform> Transforms =>
        new Transform[]
        {
            new SharpenTransform(_sharpenSlider.Value)
        };
}