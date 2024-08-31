using Eto;
using Eto.Drawing;
using Eto.Forms;
using Eto.Mac;
using Eto.Mac.Drawing;
using NAPS2.EtoForms.Layout;
using NAPS2.EtoForms.Widgets;
using NAPS2.Images.Mac;
using NAPS2.Remoting;

namespace NAPS2.EtoForms.Mac;

public class MacEtoPlatform : EtoPlatform
{
    public override bool IsMac => true;

    public override IIconProvider IconProvider { get; } = new MacIconProvider(new DefaultIconProvider());
    public override IDarkModeProvider DarkModeProvider { get; } = new MacDarkModeProvider();

    public override void InitializePlatform()
    {
        // We start the process as a background process (by setting LSBackgroundOnly in Info.plist) and only turn it
        // into a foreground process once we know we're not in worker or console mode. This ensures workers don't have
        // a chance to show in the dock.
        MacProcessHelper.TransformThisProcessToForeground();
    }

    public override Application CreateApplication()
    {
        var application = new Application(Platforms.macOS);
        ((NSApplication) application.ControlObject).Delegate = new MacAppDelegate();
        return application;
    }

    public override void Invoke(Application application, Action action)
    {
        // TODO: Eto PR to always use InvokeOnMainThread, don't execute the action directly
        // even if we're already on the main thread.
        NSApplication.SharedApplication.InvokeOnMainThread(action);
    }

    public override void AsyncInvoke(Application application, Action action)
    {
        NSApplication.SharedApplication.BeginInvokeOnMainThread(action);
    }

    public override IListView<T> CreateListView<T>(ListViewBehavior<T> behavior) =>
        new MacListView<T>(behavior);

    public override void ConfigureImageButton(Button button, ButtonFlags flags)
    {
        if (button.ImagePosition == ButtonImagePosition.Above)
        {
            var nsButton = (NSButton) button.ToNative();
            nsButton.ImageHugsTitle = true;
            nsButton.Title = Environment.NewLine + nsButton.Title;
        }
    }

    public override Bitmap ToBitmap(IMemoryImage image)
    {
        var copy = (NSImage) image.AsNsImage().Copy();
        copy.Size = new CGSize(image.Width, image.Height);
        return new Bitmap(new BitmapHandler(copy));
    }

    public override IMemoryImage FromBitmap(Bitmap bitmap)
    {
        return new MacImage(bitmap.ToNS());
    }

    public override IMemoryImage DrawHourglass(IMemoryImage image)
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

    public override float GetScaleFactor(Window window) => 2f;

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

    public override void HandleKeyDown(Control control, Func<Keys, bool> handle)
    {
        var view = control.ToNative();
        var monitor = NSEvent.AddLocalMonitorForEventsMatchingMask(NSEventMask.KeyDown, evt =>
        {
            if (ReferenceEquals(evt.Window, view.Window))
            {
                var args = evt.ToEtoKeyEventArgs();
                return handle(args.KeyData) ? null! : evt;
            }
            return evt;
        });
        control.UnLoad += (_, _) => NSEvent.RemoveMonitor(monitor);
    }

    public override void AttachMouseWheelEvent(Control control, EventHandler<MouseEventArgs> eventHandler)
    {
        var view = control.ToNative();
        var monitor = NSEvent.AddLocalMonitorForEventsMatchingMask(NSEventMask.ScrollWheel, evt =>
        {
            if (ReferenceEquals(evt.Window, view.Window) &&
                view.HitTest(evt.LocationInWindow) != null!)
            {
                var newArgs = new MouseEventArgs(
                    MouseButtons.None,
                    evt.ModifierFlags.ToEto(),
                    evt.LocationInWindow.ToEto(),
                    new SizeF((float) evt.DeltaX, (float) evt.DeltaY));
                eventHandler(control, newArgs);
                return newArgs.Handled ? null! : evt;
            }
            return evt;
        });
        control.UnLoad += (_, _) => NSEvent.RemoveMonitor(monitor);
    }

    private class MacAppDelegate : AppDelegate
    {
        public override bool OpenFile(NSApplication sender, string filename)
        {
            Task.Run(() =>
                ProcessCoordinator.CreateDefault().OpenFile(Process.GetCurrentProcess(), 100, filename));
            return true;
        }

        public override void OpenFiles(NSApplication sender, string[] filenames)
        {
            Task.Run(() =>
                ProcessCoordinator.CreateDefault().OpenFile(Process.GetCurrentProcess(), 100, filenames));
        }
    }
}