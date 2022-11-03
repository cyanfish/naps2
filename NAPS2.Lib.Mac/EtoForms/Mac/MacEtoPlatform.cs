using Eto;
using Eto.Drawing;
using Eto.Forms;
using Eto.Mac.Drawing;
using NAPS2.Images.Mac;
using sd = System.Drawing;

namespace NAPS2.EtoForms.Mac;

public class MacEtoPlatform : EtoPlatform
{
    public override bool IsMac => true;

    public override Application CreateApplication()
    {
        return new Application(Platforms.macOS);
    }

    public override IListView<T> CreateListView<T>(ListViewBehavior<T> behavior) =>
        new MacListView<T>(behavior);

    public override void ConfigureImageButton(Button button)
    {
        if (button.ImagePosition == ButtonImagePosition.Above)
        {
            var nsButton = (NSButton) button.ToNative();
            nsButton.ImageHugsTitle = true;
        }
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

    public override void SetFrame(Control container, Control control, Point location, Size size, bool inOverlay)
    {
        var rect = new CGRect(location.X, container.Height - location.Y - size.Height, size.Width, size.Height);
        var view = control.ToNative();
        view.Frame = view.GetFrameForAlignmentRect(rect);
    }

    public override Control CreateContainer()
    {
        return new NSView().ToEto();
    }

    public override void AddToContainer(Control container, Control control, bool inOverlay)
    {
        container.ToNative().AddSubview(control.ToNative());
    }

    public override Control AccessibleImageButton(Image image, String text, Action onClick,
        int xOffset = 0, int yOffset = 9)
    {
        // TODO
        return new NSView();
    }
}