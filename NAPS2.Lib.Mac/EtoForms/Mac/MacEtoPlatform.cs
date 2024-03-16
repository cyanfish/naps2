using Eto;
using Eto.Drawing;
using Eto.Forms;
using Eto.Mac;
using Eto.Mac.Drawing;
using NAPS2.EtoForms.Layout;
using NAPS2.EtoForms.Widgets;
using NAPS2.Images.Mac;

namespace NAPS2.EtoForms.Mac;

public class MacEtoPlatform : EtoPlatform
{
    public override bool IsMac => true;

    public override void InitializePlatform()
    {
        // We start the process as a background process (by setting LSBackgroundOnly in Info.plist) and only turn it
        // into a foreground process once we know we're not in worker or console mode. This ensures workers don't have
        // a chance to show in the dock.
        MacProcessHelper.TransformThisProcessToForeground();
    }

    public override Application CreateApplication()
    {
        return new Application(Platforms.macOS);
    }

    public override IListView<T> CreateListView<T>(ListViewBehavior<T> behavior) =>
        new MacListView<T>(behavior);

    public override void ConfigureImageButton(Button button, bool big)
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
        return new Bitmap(new BitmapHandler((NSImage) nsImage.Copy()));
    }

    public override IMemoryImage FromBitmap(ImageContext imageContext, Bitmap bitmap)
    {
        return new MacImage(imageContext, bitmap.ToNS());
    }

    public override IMemoryImage DrawHourglass(ImageContext imageContext, IMemoryImage image)
    {
        // TODO
        return image;
    }

    public override void SetFrame(Control container, Control control, Point location, Size size, bool inOverlay)
    {
        if (control is Button)
        {
            // EtoButton has some weird IsAutoSize logic that conflicts with frame setting unless w/h are defined
            control.Width = size.Width;
            control.Height = size.Height;
        }
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

    public override void RemoveFromContainer(Control container, Control control)
    {
        control.ToNative().RemoveFromSuperview();
    }

    public override Control AccessibleImageButton(Image image, String text, Action onClick,
        int xOffset = 0, int yOffset = 0)
    {
        return new NSButton
        {
            Title = text,
            Image = image.ToNS(),
            ImagePosition = NSCellImagePosition.ImageOnly,
            Bordered = false
        }.WithAction(onClick).ToEto();
    }

    public override LayoutElement CreateGroupBox(string title, LayoutElement content)
    {
        var titleLabel = new Label
        {
            Text = title,
            Font = NSFont.BoldSystemFontOfSize(12).ToEto()
        };
        var groupBox = new GroupBox();

        return L.Overlay(
            L.Column(
                titleLabel.Padding(top: 8).SpacingAfter(6),
                groupBox.Scale()
            ),
            L.Buffer(content, 6, 32, 6, 6)
        );
    }

    public override void SetClientSize(Window window, Size clientSize)
    {
        if (window.Loaded)
        {
            // The Eto ClientSize setter also changes the y-position and causes jumps
            var nsWindow = window.ToNative();
            nsWindow.SetContentSize(clientSize.ToNS());
        }
        else
        {
            base.SetClientSize(window, clientSize);
        }
    }
}