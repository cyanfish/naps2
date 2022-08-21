using Eto.Drawing;
using Eto.Forms;
using Eto.Mac.Drawing;
using NAPS2.Images;
using NAPS2.Images.Mac;
using sd = System.Drawing;

namespace NAPS2.EtoForms.Mac;

public class MacEtoPlatform : EtoPlatform
{
    private const int MIN_BUTTON_WIDTH = 75;
    private const int MIN_BUTTON_HEIGHT = 32;
    private const int IMAGE_PADDING = 5;

    static MacEtoPlatform()
    {
    }

    public override IListView<T> CreateListView<T>(ListViewBehavior<T> behavior) =>
        new MacListView<T>(behavior);

    public override void ConfigureImageButton(Button button)
    {
    }

    public override Bitmap ToBitmap(IMemoryImage image)
    {
        var nsImage = ((MacImage) image).NsImage;
        return new Bitmap(new BitmapHandler(nsImage));
    }

    public override IMemoryImage DrawHourglass(IMemoryImage image)
    {
        // TODO
        return image;
    }
}