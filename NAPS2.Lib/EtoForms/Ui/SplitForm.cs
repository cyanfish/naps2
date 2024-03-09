using Eto.Drawing;

namespace NAPS2.EtoForms.Ui;

public class SplitForm : UnaryImageFormBase
{
    public SplitForm(Naps2Config config, UiImageList imageList, ThumbnailController thumbnailController) :
        base(config, imageList, thumbnailController)
    {
        Icon = new Icon(1f, Icons.split.ToEtoImage());
        Title = UiStrings.Split;
    }

    protected override List<Transform> Transforms => [];
}