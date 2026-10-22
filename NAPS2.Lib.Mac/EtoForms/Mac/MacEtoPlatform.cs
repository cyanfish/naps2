using System.Runtime.CompilerServices;
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

    public override void InitializeForegroundApp()
    {
        // We start the process as a background process (by setting LSBackgroundOnly in Info.plist) and only turn it
        // into a foreground process once we know we're not in worker or console mode. This ensures workers don't have
        // a chance to show in the dock.
        MacProcessHelper.TransformThisProcessToForeground();
    }

    public override Application CreateApplicationCore()
    {
        var application = new Application(Platforms.macOS);
        ((NSApplication) application.ControlObject).Delegate = new MacAppDelegate();
        return application;
    }

    public override void SetSystemTheme()
    {
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

    public override ISettingsNavigator CreateSettingsNavigator() => new MacSettingsNavigator();

    public override void ConfigureImageButton(Button button, ButtonFlags flags)
    {
        var nsButton = (NSButton) button.ToNative();
        if (button.ImagePosition == ButtonImagePosition.Above)
        {
            nsButton.ImageHugsTitle = true;
            nsButton.Title = Environment.NewLine + nsButton.Title;
        }
        var image = nsButton.Image!;
        if (image.Representations() is [NSBitmapImageRep rep, ..])
        {
            image.Size = new CGSize(rep.PixelsWide / 2f, rep.PixelsHigh / 2f);
            nsButton.Image = image;
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
        // Use the native frame as the Eto height can be stale (e.g. if the container has been moved into a sidebar's
        // detail pane)
        var nsContainer = container.ToNative();
        var y = nsContainer.IsFlipped ? location.Y : nsContainer.Frame.Height - location.Y - size.Height;
        var rect = new CGRect(location.X, y, size.Width, size.Height);
        var view = control.ToNative();
        view.Frame = view.GetFrameForAlignmentRect(rect);
    }

    public override Control CreateContainer()
    {
        return new NSView().ToEto();
    }

    public override Size GetContainerSize(Control container) => container.ToNative().Frame.Size.ToEtoSize();

    public override void AttachContainerSizeChanged(Window window, Control container, Action action)
    {
        var view = container.ToNative();
        var lastSize = view.Frame.Size;
        view.PostsFrameChangedNotifications = true;
        var observer = NSNotificationCenter.DefaultCenter.AddObserver(NSView.FrameChangedNotification, _ =>
        {
            // The notification is also posted when the view moves, so only update if the size changes
            if (view.Frame.Size != lastSize)
            {
                lastSize = view.Frame.Size;
                action();
            }
        }, view);
        window.Closed += (_, _) => NSNotificationCenter.DefaultCenter.RemoveObserver(observer);
    }

    public override void AddToContainer(Control container, Control control, bool inOverlay)
    {
        container.ToNative().AddSubview(control.ToNative());
    }

    public override void RemoveFromContainer(Control container, Control control)
    {
        control.ToNative().RemoveFromSuperview();
    }

    public override Control CreateScrollContainer()
    {
        return new NSScrollView
        {
            HasVerticalScroller = true,
            AutohidesScrollers = true,
            DrawsBackground = false,
            // Use a flipped view so the content is anchored to the top
            DocumentView = new FlippedView()
        }.ToEto();
    }

    public override Control GetScrollContentContainer(Control scrollContainer)
    {
        return ((NSScrollView) scrollContainer.ToNative()).DocumentView!.ToEto();
    }

    public override void SetScrollContentSize(Control scrollContainer, Size size)
    {
        var documentView = ((NSScrollView) scrollContainer.ToNative()).DocumentView!;
        documentView.SetFrameSize(new CGSize(size.Width, size.Height));
    }

    public override int GetVerticalScrollBarWidth(Control scrollContainer)
    {
        var scrollView = (NSScrollView) scrollContainer.ToNative();
        if (scrollView.ScrollerStyle == NSScrollerStyle.Overlay)
        {
            return 0;
        }
        return (int) Math.Ceiling(NSScroller.GetScrollerWidth(NSControlSize.Regular, NSScrollerStyle.Legacy));
    }

    // Windows with a sidebar
    private readonly ConditionalWeakTable<Window, SidebarState> _sidebars = new();

    public override bool SupportsWindowSidebar => true;

    public override void AttachWindowSidebar(Window window, Control sidebar, WindowSidebarOptions options)
    {
        var nsWindow = window.ToNative();
        var etoContentView = nsWindow.ContentView!;
        var layoutContainer = ((IFormBase) window).LayoutController.Container.ToNative();
        var sidebarView = sidebar.ToNative();
        var frame = nsWindow.Frame;

        // A split view sidebar item gets the standard sidebar appearance (e.g. Liquid Glass on macOS 26)
        var sidebarItem = NSSplitViewItem.CreateSidebar(new NSViewController { View = sidebarView });
        if (options.MinWidth is { } minWidth)
        {
            sidebarItem.MinimumThickness = minWidth;
        }
        if (options.MaxWidth is { } maxWidth)
        {
            sidebarItem.MaximumThickness = maxWidth;
        }
        sidebarItem.CanCollapse = options.CanHide;
        var detailView = new NSView();
        var splitViewController = new NSSplitViewController();
        var detailItem = NSSplitViewItem.FromViewController(new NSViewController { View = detailView });
        if (options.MinContentWidth is { } minContentWidth)
        {
            detailItem.MinimumThickness = minContentWidth;
        }
        splitViewController.AddSplitViewItem(sidebarItem);
        splitViewController.AddSplitViewItem(detailItem);
        sidebarItem.Collapsed = !options.Visible;

        // Extend the sidebar under the title bar
        nsWindow.StyleMask |= NSWindowStyle.FullSizeContentView;
        nsWindow.TitlebarAppearsTransparent = true;
        nsWindow.ContentViewController = splitViewController;

        etoContentView.TranslatesAutoresizingMaskIntoConstraints = false;
        detailView.AddSubview(etoContentView);
        NSLayoutConstraint.ActivateConstraints([
            options.ContentUnderTitleBar
                ? etoContentView.TopAnchor.ConstraintEqualTo(detailView.TopAnchor)
                : etoContentView.TopAnchor.ConstraintEqualTo(((NSLayoutGuide) nsWindow.ContentLayoutGuide!).TopAnchor),
            etoContentView.BottomAnchor.ConstraintEqualTo(detailView.BottomAnchor),
            etoContentView.LeadingAnchor.ConstraintEqualTo(detailView.LeadingAnchor),
            etoContentView.TrailingAnchor.ConstraintEqualTo(detailView.TrailingAnchor)
        ]);

        // Eto sizes its content views to the window's full content size, so we need to keep them sized to the detail
        // pane instead
        void FitEtoContent()
        {
            for (var view = etoContentView.Subviews.FirstOrDefault(); view != null; view = view.Subviews.FirstOrDefault())
            {
                view.Frame = view.Superview!.Bounds;
                if (view == layoutContainer) break;
            }
            ((IFormBase) window).LayoutController.Invalidate();
        }
        var lastWidth = options.Width;
        void SidebarResized()
        {
            // The sidebar view itself may be inset within the split view pane
            var pane = splitViewController.SplitView.ArrangedSubviews[0];
            var sidebarWidth = (int) Math.Round(pane.Frame.Width);
            if (!sidebarItem.Collapsed && sidebarWidth > 0 && sidebarWidth != lastWidth)
            {
                lastWidth = sidebarWidth;
                options.WidthChanged?.Invoke(sidebarWidth);
            }
        }
        // Fit as soon as the detail pane resizes rather than waiting for Eto to resize its views, to avoid flicker
        etoContentView.PostsFrameChangedNotifications = true;
        layoutContainer.PostsFrameChangedNotifications = true;
        sidebarView.PostsFrameChangedNotifications = true;
        NSObject[] observers =
        [
            NSNotificationCenter.DefaultCenter.AddObserver(NSView.FrameChangedNotification, _ => FitEtoContent(),
                etoContentView),
            NSNotificationCenter.DefaultCenter.AddObserver(NSView.FrameChangedNotification, _ =>
            {
                if (layoutContainer.Frame != layoutContainer.Superview?.Bounds) FitEtoContent();
            }, layoutContainer),
            NSNotificationCenter.DefaultCenter.AddObserver(NSView.FrameChangedNotification, _ => SidebarResized(),
                sidebarView)
        ];
        // The sidebar can also be collapsed by dragging the divider, not just by SetWindowSidebarVisible
        var collapsedObserver = sidebarItem.AddObserver("collapsed", NSKeyValueObservingOptions.New,
            _ => options.VisibleChanged?.Invoke(!sidebarItem.Collapsed));
        window.Closed += (_, _) =>
        {
            NSNotificationCenter.DefaultCenter.RemoveObservers(observers);
            collapsedObserver.Dispose();
        };
        _sidebars.AddOrUpdate(window, new SidebarState(etoContentView, splitViewController, sidebarItem));

        if (options.Visible)
        {
            frame.Width += options.Width;
        }
        // Setting the content view controller resizes the window to fit it, so the frame always needs to be set
        nsWindow.SetFrame(frame, true);
        if (options.Visible)
        {
            splitViewController.SplitView.SetPositionOfDivider(options.Width, 0);
        }
        FitEtoContent();
    }

    public override void SetWindowSidebarVisible(Window window, bool visible)
    {
        if (_sidebars.TryGetValue(window, out var state) && state.SidebarItem.Collapsed == visible)
        {
            // Animates the change
            state.SplitViewController.ToggleSidebar(null);
        }
    }

    private record SidebarState(
        NSView ContentView,
        NSSplitViewController SplitViewController,
        NSSplitViewItem SidebarItem);

    public override Size GetClientSize(Window window, bool excludeToolbars = false)
    {
        if (_sidebars.TryGetValue(window, out var sidebar))
        {
            return sidebar.ContentView.Frame.Size.ToEtoSize();
        }
        return base.GetClientSize(window, excludeToolbars);
    }

    public override void SetMinimumClientSize(Window window, Size minSize)
    {
        if (_sidebars.TryGetValue(window, out var sidebar))
        {
            window.ToNative().ContentMinSize = (minSize + GetSidebarSizeDelta(window, sidebar.ContentView)).ToNS();
            return;
        }
        base.SetMinimumClientSize(window, minSize);
    }

    private Size GetSidebarSizeDelta(Window window, NSView contentView) =>
        window.ToNative().ContentView!.Frame.Size.ToEtoSize() - contentView.Frame.Size.ToEtoSize();

    private class FlippedView : NSView
    {
        public override bool IsFlipped => true;
    }

    public override float GetScaleFactor(Window window) => 2f;

    public override void ConfigureDonateButton(Button button)
    {
        var native = (NSButton) button.ToNative();
        native.Bordered = false;
    }

    public override void SetAccessibleName(Control control, string name) =>
        control.ToNative().AccessibilityLabel = name;

    public override LayoutElement CreateGroupBox(string title, LayoutElement content, GroupBoxHeaderStyle headerStyle)
    {
        bool bold = headerStyle == GroupBoxHeaderStyle.Bold;
        var titleLabel = new Label
        {
            Text = title,
            Font = (bold ? NSFont.BoldSystemFontOfSize(12) : NSFont.SystemFontOfSize(12)).ToEto()
        };
        var groupBox = new GroupBox();

        // Bold headers have a bit more spacing above them
        return L.Overlay(
            L.Column(
                titleLabel.SpacingAfter(6),
                groupBox.Scale()
            ),
            L.Buffer(content, 6, 24, 6, 6)
        ).SpacingBefore(bold ? 8 : 0);
    }

    public override void SetClientSize(Window window, Size clientSize)
    {
        if (_sidebars.TryGetValue(window, out var sidebar))
        {
            window.ToNative().SetContentSize((clientSize + GetSidebarSizeDelta(window, sidebar.ContentView)).ToNS());
            return;
        }
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
                var args = ToEtoKeyEventArgs(evt);
                return handle(args.KeyData) ? null! : evt;
            }
            return evt;
        });
        control.UnLoad += (_, _) => NSEvent.RemoveMonitor(monitor);
    }

    private static KeyEventArgs ToEtoKeyEventArgs(NSEvent theEvent)
    {
        // Custom impl until fixed: https://github.com/picoe/Eto/issues/3002
        char keyChar = !string.IsNullOrEmpty(theEvent.Characters) ? theEvent.Characters[0] : '\0';
        Keys key = KeyMap.Convert(theEvent.CharactersIgnoringModifiers, 0);
        if (key == Keys.None)
        {
            key = KeyMap.MapKey(theEvent.KeyCode, theEvent.ModifierFlags);
        }
        KeyEventArgs kpea;
        Keys modifiers = theEvent.ModifierFlags.ToEto();
        key |= modifiers;

        KeyEventType keyEventType = theEvent.Type == NSEventType.KeyUp ? KeyEventType.KeyUp : KeyEventType.KeyDown;

        if (key != Keys.None)
        {
            if (((modifiers & ~(Keys.Shift | Keys.Alt)) == 0))
                kpea = new KeyEventArgs(key, keyEventType, keyChar);
            else
                kpea = new KeyEventArgs(key, keyEventType);
        }
        else
        {
            kpea = new KeyEventArgs(key, keyEventType, keyChar);
        }
        return kpea;
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

    public override void ConfigureFileDialog(FileDialog fileDialog)
    {
        if (fileDialog is SaveFileDialog sd)
        {
            var savePanel = (NSSavePanel) sd.ControlObject;
            // Ensure the correct extension is added on save
            savePanel.AllowsOtherFileTypes = false;
        }
    }
}