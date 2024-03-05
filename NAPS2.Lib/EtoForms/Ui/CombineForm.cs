using Eto.Drawing;
using NAPS2.EtoForms.Widgets;

namespace NAPS2.EtoForms.Ui;

public class CombineForm : ImageFormBase
{
    public CombineForm(Naps2Config config, UiImageList imageList, ThumbnailController thumbnailController) :
        base(config, imageList, thumbnailController)
    {
        Icon = new Icon(1f, Icons.combine.ToEtoImage());
        Title = UiStrings.Combine;
    }

    protected override List<Transform> Transforms => [];
}