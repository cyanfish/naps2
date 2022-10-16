using Eto;
using Eto.Drawing;
using Eto.Forms;
using Eto.Mac.Drawing;
using NAPS2.Images.Mac;
using sd = System.Drawing;

namespace NAPS2.EtoForms.Mac;

public class MacEtoPlatform : EtoPlatform
{
    private const int MIN_BUTTON_WIDTH = 75;
    private const int MIN_BUTTON_HEIGHT = 32;
    private const int IMAGE_PADDING = 5;

    public override bool IsMac => true;

    public override Application CreateApplication()
    {
        return new Application(Platforms.macOS);
    }

    public override IListView<T> CreateListView<T>(ListViewBehavior<T> behavior) =>
        new MacListView<T>(behavior);

    public override void ConfigureImageButton(Button button)
    {
    }

    public override Bitmap ToBitmap(IMemoryImage image)
    {
        // TODO: This is kind of busted in terms of image size.
        // Eto seems to use NsImage.Size instead of Rep.PixelsWide/High.
        // That can be incorrect (see MacImageTransformer.DoScale).
        var nsImage = ((MacImage) image).NsImage;
        return new Bitmap(new BitmapHandler(nsImage));
    }

    public override IMemoryImage DrawHourglass(ImageContext imageContext, IMemoryImage image)
    {
        // TODO
        return image;
    }

    public override void SetFrame(Control container, Control control, Point location, Size size)
    {
        var rect = new CGRect(location.X, container.Height - location.Y - size.Height, size.Width, size.Height);
        var view = control.ToNative();
        view.Frame = view.GetFrameForAlignmentRect(rect);
    }

    public override Control CreateContainer()
    {
        return new NSView().ToEto();
    }

    public override void AddToContainer(Control container, Control control)
    {
        container.ToNative().AddSubview(control.ToNative());
    }
}