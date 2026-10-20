using System.Threading;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Eto.Drawing;
using Eto.Forms;
using Eto.Wpf;
using Eto.Wpf.Drawing;
using Eto.Wpf.Forms;
using NAPS2.EtoForms.Layout;
using NAPS2.EtoForms.Widgets;
using NAPS2.EtoForms.WinForms;
using NAPS2.Images.Wpf;
using SW = System.Windows;
using SWC = System.Windows.Controls;
using SWM = System.Windows.Media;

namespace NAPS2.EtoForms.Wpf;

public class WpfEtoPlatform : EtoPlatform
{
    private const int SMALL_BUTTON_PADDING = 7;

    private static readonly Size MinImageButtonSize = new(75, 32);

    public override bool IsWpf => true;

    public override IIconProvider IconProvider { get; } = new DefaultIconProvider();

    public override IDarkModeProvider DarkModeProvider { get; } = new WindowsDarkModeProvider();

    public override Application CreateApplicationCore()
    {
        // Eto.Wpf reuses an existing System.Windows.Application if there is one, so we create it first to configure
        // the Fluent theme before any windows are created.
        _ = new SW.Application { ShutdownMode = SW.ShutdownMode.OnExplicitShutdown };
        var application = new Application(Eto.Platforms.Wpf);
        SynchronizationContext.SetSynchronizationContext(
            new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher));
        SetSystemTheme();
        return application;
    }

    public override void SetSystemTheme()
    {
        if (SW.Application.Current is { } app)
        {
            app.ThemeMode = ColorScheme.DarkMode ? SW.ThemeMode.Dark : SW.ThemeMode.Light;
        }
    }

    public override IListView<T> CreateListView<T>(ListViewBehavior<T> behavior) =>
        throw new NotImplementedException();

    public override ISettingsNavigator CreateSettingsNavigator() => throw new NotImplementedException();

    public override void ConfigureImageButton(Button button, ButtonFlags flags)
    {
        var native = (SWC.Control) button.ToNative();
        if (string.IsNullOrEmpty(button.Text))
        {
            native.Padding = new SW.Thickness(SMALL_BUTTON_PADDING - 1, SMALL_BUTTON_PADDING,
                SMALL_BUTTON_PADDING - 1, SMALL_BUTTON_PADDING);
            SetIconProps(button, 16);
        }
        else
        {
            native.Padding = new SW.Thickness(14, 5, 16, 6);
            button.MinimumSize = MinImageButtonSize;
            int iconSize = flags.HasFlag(ButtonFlags.LargeIcon) ? 32 : 16;
            if (button.ImagePosition == ButtonImagePosition.Left && iconSize == 16)
            {
                SetIconProps(button, iconSize,
                    new SW.Setter(SW.FrameworkElement.MarginProperty, new SW.Thickness(0, 2, 4, 0)),
                    new SW.Setter(SW.FrameworkElement.VerticalAlignmentProperty, SW.VerticalAlignment.Top));
            }
            else if (button.ImagePosition == ButtonImagePosition.Left)
            {
                SetIconProps(button, iconSize,
                    new SW.Setter(SW.FrameworkElement.MarginProperty, new SW.Thickness(0, 0, 4, 0)));
            }
            else
            {
                // Eto top-aligns the content, so center it for buttons taller than their natural height
                native.VerticalContentAlignment = SW.VerticalAlignment.Center;
                SetIconProps(button, iconSize);
            }
        }
    }

    private static void SetIconProps(Button button, int size, params SW.Setter[] extraSetters)
    {
        var style = new SW.Style(typeof(SWC.Image))
        {
            Setters =
            {
                new SW.Setter(SW.FrameworkElement.WidthProperty, (double) size),
                new SW.Setter(SW.FrameworkElement.HeightProperty, (double) size),
                new SW.Setter(SWM.RenderOptions.BitmapScalingModeProperty, SWM.BitmapScalingMode.HighQuality)
            }
        };
        foreach (var setter in extraSetters)
        {
            style.Setters.Add(setter);
        }
        button.ToNative().Resources[typeof(SWC.Image)] = style;
    }

    public override Bitmap ToBitmap(IMemoryImage image) => new(new BitmapHandler(image.ToBitmapSource()));

    public override IMemoryImage FromBitmap(Bitmap bitmap) => new WpfImage(bitmap.ToWpf());

    public override IMemoryImage DrawHourglass(IMemoryImage image)
    {
        var visual = new SWM.DrawingVisual();
        using (var dc = visual.RenderOpen())
        {
            dc.PushOpacity(0.3);
            dc.DrawImage(image.ToBitmapSource(), new SW.Rect(0, 0, image.Width, image.Height));
            dc.Pop();
            var hourglass = BitmapFrame.Create(new MemoryStream(Icons.hourglass_grey), BitmapCreateOptions.None,
                BitmapCacheOption.OnLoad);
            // Use whole pixels so the icon isn't blurred
            int left = (image.Width - 32) / 2, top = (image.Height - 32) / 2;
            dc.DrawImage(hourglass, new SW.Rect(left, top, 32, 32));
        }
        var bitmap = new RenderTargetBitmap(image.Width, image.Height, 96, 96, SWM.PixelFormats.Pbgra32);
        bitmap.Render(visual);
        image.Dispose();
        return new WpfImage(bitmap);
    }

    private static FramePanel GetPanel(Control container) => (FramePanel) container.ControlObject;

    public override Control CreateContainer() => new FramePanel { ClipToBounds = true }.ToEto();

    public override void AddToContainer(Control container, Control control, bool inOverlay)
    {
        // Signal Eto that it doesn't need to manage width/height
        control.GetWpfFrameworkElement()?.SetScale(true, true);
        var native = control.ToNative();
        if (inOverlay)
        {
            // Keeps overlays above any non-overlay controls added later
            SWC.Panel.SetZIndex(native, 1);
        }
        GetPanel(container).Children.Add(native);
    }

    public override void RemoveFromContainer(Control container, Control control)
    {
        GetPanel(container).Children.Remove(control.ToNative());
    }

    public override void SetFrame(Control container, Control control, Point location, Size size, bool inOverlay)
    {
        FramePanel.SetFrame(control.ToNative(), new SW.Rect(location.X, location.Y, size.Width, size.Height));
    }

    public override SizeF GetPreferredSize(Control control, SizeF availableSpace)
    {
        var native = control.ToNative();
        try
        {
            native.Measure(new SW.Size(double.PositiveInfinity, double.PositiveInfinity));
            var desired = native.DesiredSize;
            var availableWidth = ToMeasureLength(availableSpace.Width);
            if (desired.Width > availableWidth && control is Label { Wrap: not WrapMode.None })
            {
                native.Measure(new SW.Size(availableWidth, double.PositiveInfinity));
                desired = native.DesiredSize;
            }
            else if (control is Label or LinkButton or CheckBox or RadioButton)
            {
                // With layout rounding, the desired size can be rounded down (e.g. 145.3 -> 145), and Eto.Wpf's text
                // in these controls wraps (and the second line is clipped) if it's even slightly too narrow
                desired.Width += 1;
            }
            return new SizeF((float) Math.Ceiling(desired.Width), (float) Math.Ceiling(desired.Height));
        }
        finally
        {
            // Measure again at its frame size (if it has one) in the next layout pass
            (SWM.VisualTreeHelper.GetParent(native) as SW.UIElement)?.InvalidateMeasure();
        }
    }

    private static double ToMeasureLength(float length) =>
        length >= LayoutController.MAX_SIZE ? double.PositiveInfinity : Math.Max(length, 0);

    public override Control CreateScrollContainer()
    {
        var scrollViewer = new SWC.ScrollViewer
        {
            VerticalScrollBarVisibility = SWC.ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = SWC.ScrollBarVisibility.Disabled,
            Content = new FramePanel()
        };
        return scrollViewer.ToEto();
    }

    public override Control GetScrollContentContainer(Control scrollContainer)
    {
        var scrollViewer = (SWC.ScrollViewer) scrollContainer.ControlObject;
        return ((FramePanel) scrollViewer.Content).ToEto();
    }

    public override void SetScrollContentSize(Control scrollContainer, Size size)
    {
        var scrollViewer = (SWC.ScrollViewer) scrollContainer.ControlObject;
        var panel = (FramePanel) scrollViewer.Content;
        panel.Width = size.Width;
        panel.Height = size.Height;
    }

    public override int GetVerticalScrollBarWidth(Control scrollContainer) =>
        (int) Math.Ceiling(SW.SystemParameters.VerticalScrollBarWidth);

    public override void ConfigureDonateButton(Button button) => throw new NotImplementedException();
}
