using System.Globalization;
using System.Threading;
using System.Windows.Automation;
using System.Windows.Input;
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
using NAPS2.Platform.Windows;
using MouseEventArgs = Eto.Forms.MouseEventArgs;
using Keyboard = System.Windows.Input.Keyboard;
using SW = System.Windows;
using SWC = System.Windows.Controls;
using SWM = System.Windows.Media;

namespace NAPS2.EtoForms.Wpf;

public class WpfEtoPlatform : EtoPlatform
{
    private const int SMALL_BUTTON_PADDING = 7;
    private const int SMALL_TOOLBAR_BUTTON_PADDING = 4;
    private const double ZOOM_BUTTON_OPACITY = 0.85;
    private const double NARROW_TEXT_BOX_WIDTH = 100;

    private static readonly Size MinImageButtonSize = new(75, 32);

    public override bool IsWpf => true;

    public override IIconProvider IconProvider { get; } = new FluentIconProvider(new DefaultIconProvider());

    public override IDarkModeProvider DarkModeProvider { get; } = new WindowsDarkModeProvider();

    public override Application CreateApplicationCore()
    {
        // Eto.Wpf reuses an existing System.Windows.Application if there is one, so we create it first to configure
        // the Fluent theme before any windows are created.
        _ = new SW.Application { ShutdownMode = SW.ShutdownMode.OnExplicitShutdown };
        var application = new Application(Eto.Platforms.Wpf);
        // Drawables paint their own content (e.g. the preview image), which a right-to-left window would mirror
        Eto.Style.Add<Eto.Wpf.Forms.Controls.DrawableHandler>(null,
            handler => handler.Control.FlowDirection = SW.FlowDirection.LeftToRight);
        SynchronizationContext.SetSynchronizationContext(
            new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher));
        SetSystemTheme();
        HideClearButtonInNarrowTextBoxes();
        return application;
    }

    private static void HideClearButtonInNarrowTextBoxes()
    {
        // The Fluent clear button covers too much of the text in narrow text boxes (e.g. for numbers). A local value
        // takes precedence over the template triggers that show it.
        SW.EventManager.RegisterClassHandler(typeof(SWC.TextBox), SW.FrameworkElement.SizeChangedEvent,
            new SW.SizeChangedEventHandler((sender, _) =>
            {
                var textBox = (SWC.TextBox) sender;
                if (textBox.Template?.FindName("DeleteButton", textBox) is not SW.UIElement deleteButton) return;
                if (textBox.ActualWidth < NARROW_TEXT_BOX_WIDTH)
                {
                    deleteButton.Visibility = SW.Visibility.Collapsed;
                }
                else
                {
                    deleteButton.ClearValue(SW.UIElement.VisibilityProperty);
                }
            }));
    }

    public override void SetSystemTheme()
    {
        if (SW.Application.Current is { } app)
        {
            app.ThemeMode = ColorScheme.DarkMode ? SW.ThemeMode.Dark : SW.ThemeMode.Light;
            ApplyFluentStylesToEtoControls(app);
            MakeSplittersUnfocusable(app);
            MakeComboBoxItemsHitTestable(app);
        }
    }

    /// <summary>
    /// Makes Eto.Wpf controls pick up the Fluent theme. There are two problems:
    /// 1. Fluent provides implicit styles keyed by type (e.g. Button), which WPF only applies to that exact type, while
    ///    Eto.Wpf uses its own subclasses (e.g. EtoButton).
    /// 2. Eto's themes/generic.xaml (merged into the application resources) has its own implicit styles without BasedOn
    ///    (e.g. for EtoButton, CheckBox, EtoFormWindow), which replace the Fluent styles entirely.
    /// So for each Eto control type (and each type Eto styles), we add an application-level style based on the Fluent
    /// style of the nearest base type, merged with Eto's own setters/triggers.
    /// </summary>
    private static void ApplyFluentStylesToEtoControls(SW.Application app)
    {
        var etoDictionaries = app.Resources.MergedDictionaries.Where(IsEtoDictionary).SelectMany(Flatten).ToList();
        var themeDictionaries = app.Resources.MergedDictionaries.Where(d => !IsEtoDictionary(d)).Reverse().ToList();

        var types = GetEtoControlTypes().Concat(etoDictionaries.SelectMany(d => d.Keys.OfType<Type>())).Distinct();
        foreach (var type in types)
        {
            // Remove the style from any previous call (e.g. when the theme changes) so it isn't found as a theme style
            app.Resources.Remove(type);
            if (FindThemeStyle(themeDictionaries, type) is not { } themeStyle) continue;
            var style = new SW.Style(type, themeStyle);
            foreach (var etoDictionary in etoDictionaries.Where(d => d.Contains(type)))
            {
                CopyEtoStyle((SW.Style) etoDictionary[type]!, style);
            }
            if (typeof(SWC.CheckBox).IsAssignableFrom(type) || typeof(SWC.RadioButton).IsAssignableFrom(type))
            {
                MakeToggleCompact(style);
            }
            app.Resources[type] = style;
        }
    }

    private static void MakeSplittersUnfocusable(SW.Application app)
    {
        // The splitter randomly stealing focus when items are deleted is bad UX for the desktop form
        ExtendThemeStyle(app, typeof(SWC.GridSplitter), new SW.Setter(SW.UIElement.FocusableProperty, false));
    }

    private static void MakeComboBoxItemsHitTestable(SW.Application app)
    {
        // Fix combobox items not responding to mouseover outside the text content
        var borderStyle = new SW.Style(typeof(SWC.Border))
        {
            Setters = { new SW.Setter(SWC.Border.BackgroundProperty, SWM.Brushes.Transparent) }
        };
        ExtendThemeStyle(app, typeof(SWC.ComboBoxItem), new SW.EventSetter(SW.FrameworkElement.LoadedEvent,
            new SW.RoutedEventHandler((sender, _) =>
            {
                var item = (SWC.ComboBoxItem) sender;
                if (item.Template?.FindName("ContentBorder", item) is SWC.Border border)
                {
                    border.Style = borderStyle;
                }
            })));
    }

    private static void ExtendThemeStyle(SW.Application app, Type type, SW.SetterBase setter)
    {
        app.Resources.Remove(type);
        app.Resources[type] = new SW.Style(type, app.TryFindResource(type) as SW.Style) { Setters = { setter } };
    }

    private static bool IsEtoDictionary(SW.ResourceDictionary dict) =>
        dict.Source?.OriginalString.Contains("Eto.Wpf") == true;

    private static IEnumerable<SW.ResourceDictionary> Flatten(SW.ResourceDictionary dict) =>
        dict.MergedDictionaries.SelectMany(Flatten).Prepend(dict);

    private static IEnumerable<Type> GetEtoControlTypes()
    {
        Type[] types;
        try
        {
            types = typeof(Eto.Wpf.Platform).Assembly.GetTypes();
        }
        catch (System.Reflection.ReflectionTypeLoadException ex)
        {
            // Optional dependencies aren't present
            types = ex.Types.OfType<Type>().ToArray();
        }
        return types.Where(t =>
            typeof(SW.FrameworkElement).IsAssignableFrom(t) && t is { IsAbstract: false, ContainsGenericParameters: false });
    }

    private static SW.Style? FindThemeStyle(List<SW.ResourceDictionary> themeDictionaries, Type type)
    {
        for (var t = type; t != null && t != typeof(SW.FrameworkElement); t = t.BaseType)
        {
            if (themeDictionaries.Select(d => d[t]).OfType<SW.Style>().FirstOrDefault() is { } style) return style;
        }
        return null;
    }

    private static void CopyEtoStyle(SW.Style etoStyle, SW.Style style)
    {
        foreach (var setter in etoStyle.Setters.OfType<SW.Setter>())
        {
            if (setter.Property == SWC.Control.BackgroundProperty || setter.Property == SWC.Control.PaddingProperty)
            {
                // Eto backgrounds and padding don't fit the Fluent look
                continue;
            }
            style.Setters.Add(new SW.Setter(setter.Property, setter.Value, setter.TargetName));
        }
        foreach (var trigger in etoStyle.Triggers)
        {
            style.Triggers.Add(trigger);
        }
    }

    private static void MakeToggleCompact(SW.Style style)
    {
        // Check boxes and radio buttons are too big by default for NAPS2's layouts
        style.Setters.Add(new SW.Setter(SW.FrameworkElement.MinHeightProperty, 0.0));
        style.Setters.Add(new SW.Setter(SW.FrameworkElement.MinWidthProperty, 0.0));
        style.Setters.Add(new SW.Setter(SWC.Control.PaddingProperty, new SW.Thickness(6, 0, 0, 0)));
        style.Setters.Add(new SW.Setter(SWC.Control.VerticalContentAlignmentProperty, SW.VerticalAlignment.Center));
        // Fallback for controls that aren't measured by our layout
        style.Setters.Add(new SW.EventSetter(SW.FrameworkElement.LoadedEvent,
            new SW.RoutedEventHandler((sender, _) => ApplyCompactToggleTemplate((SW.FrameworkElement) sender))));
    }

    private static void ApplyCompactToggleTemplate(SW.FrameworkElement native)
    {
        // The size of the checkbox square / radio button outer circle
        const int boxSize = 16;
        // The size of the check mark and the radio button inner dot
        const int checkGlyphSize = 11;
        const int radioGlyphSize = 10;

        if (native is not (SWC.CheckBox or SWC.RadioButton)) return;
        var toggle = (SWC.Control) native;
        toggle.ApplyTemplate();
        if (toggle.Template == null) return;
        object? Part(string name) => toggle.Template.FindName(name, toggle);
        if (Part("ControlBorderIconPresenter") is SW.FrameworkElement box)
        {
            box.Width = box.Height = boxSize;
        }
        if (Part("ControlIcon") is SWC.TextBlock checkGlyph)
        {
            // The check mark is a font glyph
            checkGlyph.FontSize = checkGlyphSize;
        }
        if (Part("OuterEllipse") is SW.FrameworkElement { Parent: SWC.Grid { Parent: SWC.Grid rootGrid } ellipseGrid })
        {
            // Radio button: the circles are in a fixed-height cell in a fixed-width column
            ellipseGrid.Height = double.NaN;
            ellipseGrid.VerticalAlignment = SW.VerticalAlignment.Center;
            rootGrid.ColumnDefinitions[0].Width = new SW.GridLength(boxSize);
            foreach (var ellipse in ellipseGrid.Children.OfType<SW.Shapes.Ellipse>())
            {
                ellipse.Width = ellipse.Height = ellipse.Name == "CheckGlyph" ? radioGlyphSize : boxSize;
            }
        }
    }

    public override IListView<T> CreateListView<T>(ListViewBehavior<T> behavior) => new WpfListView<T>(behavior);

    public override ISettingsNavigator CreateSettingsNavigator() => new WpfSettingsNavigator();

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

    private const int GROUP_BOX_HEADER_HEIGHT = 26;
    private const int GROUP_BOX_BOLD_HEADER_SPACING = 8;
    private const int GROUP_BOX_PADDING = 8;

    public override LayoutElement CreateGroupBox(string title, LayoutElement content, GroupBoxHeaderStyle headerStyle)
    {
        var groupBox = new GroupBox { Text = title };
        var native = (SWC.GroupBox) groupBox.ControlObject;
        native.Template = XamlTemplates.CardGroupBoxTemplate;
        bool bold = headerStyle == GroupBoxHeaderStyle.Bold;
        if (bold)
        {
            ((SWC.Control) native.Header).FontWeight = SW.FontWeights.SemiBold;
        }
        int p = GROUP_BOX_PADDING;
        return L.Overlay(groupBox, L.Buffer(content, p, GROUP_BOX_HEADER_HEIGHT + p, p, p))
            .SpacingBefore(bold ? GROUP_BOX_BOLD_HEADER_SPACING : 0);
    }

    public override void ConfigureSmallToolBar(ToolBar toolBar)
    {
        // The Fluent toolbar button template has a fixed padding (12,6) meant for text buttons, so icon buttons take up
        // too much space
        foreach (var item in toolBar.Items)
        {
            if (item.ControlObject is SWC.Control native)
            {
                native.Loaded += (_, _) =>
                {
                    if (SWM.VisualTreeHelper.GetChildrenCount(native) > 0 &&
                        SWM.VisualTreeHelper.GetChild(native, 0) is SWC.Border border)
                    {
                        border.Padding = new SW.Thickness(SMALL_TOOLBAR_BUTTON_PADDING);
                    }
                };
            }
        }
    }

    public override void ConfigureDonateButton(Button button)
    {
        var native = (SWC.Control) button.ToNative();
        native.BorderBrush = new SWM.SolidColorBrush(SWM.Color.FromRgb(0xfb, 0xad, 0x5f));
        native.BorderThickness = new SW.Thickness(2);
    }

    public override void SetAccessibleName(Control control, string name) =>
        AutomationProperties.SetName(control.ToNative(), name);

    public override void ConfigureZoomButton(Button button, string icon)
    {
        var native = (SWC.Control) button.ToNative();
        button.Text = "";
        button.Size = new Size(32, 32);
        native.Padding = new SW.Thickness(SMALL_BUTTON_PADDING);
        native.MinWidth = native.MinHeight = 0;
        SetIconProps(button, 16);
        AttachDpiDependency(button, scale => button.Image = IconProvider.GetIcon(icon, IconScenario.Button, scale));
        // These buttons are over the thumbnails, so make them (mostly) opaque, with a higher contrast border
        void UpdateBackground(object? sender, EventArgs e) => SetOpaqueButtonBackground(native);
        SetOpaqueButtonBackground(native);
        native.Loaded += (_, _) => ColorScheme.ColorSchemeChanged += UpdateBackground;
        native.Unloaded += (_, _) => ColorScheme.ColorSchemeChanged -= UpdateBackground;
    }

    private static void SetOpaqueButtonBackground(SWC.Control native)
    {
        // Look up the theme brushes from the application, as the button's own resources have the previous overrides
        var app = SW.Application.Current;
        var back = ((SWM.SolidColorBrush) app.FindResource("SolidBackgroundFillColorBaseBrush")!).Color;
        var border = app.FindResource("ControlStrongStrokeColorDefaultBrush");
        foreach (var state in new[] { "", "PointerOver", "Pressed", "Disabled" })
        {
            var fore = ((SWM.SolidColorBrush) app.FindResource($"ButtonBackground{state}")!).Color;
            double a = fore.A / 255.0;
            byte Blend(byte f, byte b) => (byte) Math.Round(f * a + b * (1 - a));
            var brush = new SWM.SolidColorBrush(SWM.Color.FromArgb((byte) Math.Round(255 * ZOOM_BUTTON_OPACITY),
                Blend(fore.R, back.R), Blend(fore.G, back.G), Blend(fore.B, back.B)));
            brush.Freeze();
            native.Resources[$"ButtonBackground{state}"] = brush;
            native.Resources[$"ButtonBorderBrush{state}"] = border;
        }
    }

    public override Bitmap ToBitmap(IMemoryImage image) => new(new BitmapHandler(image.ToDisplayBitmapSource()));

    public override IMemoryImage FromBitmap(Bitmap bitmap) => new WpfImage(bitmap.ToWpf());

    public override void SetClipboardImage(Clipboard clipboard, ProcessedImage processedImage, IMemoryImage memoryImage)
    {
        // We also add the JPEG/PNG format to the clipboard as some applications care about the actual format
        // https://github.com/cyanfish/naps2/issues/264
        var jpegOrPng = ImageExportHelper.SaveSmallestFormatToMemoryStream(memoryImage,
            processedImage.Metadata.Lossless, -1, out var fileFormat);
        var handler = (ClipboardHandler) clipboard.Handler;
        // Note this only updates the DataObject, the clipboard is set along with the image below
        handler.Control.SetData(fileFormat == ImageFileFormat.Jpeg ? "JFIF" : "PNG", jpegOrPng);

        // Unlike ToBitmap (for display), keep the image's resolution
        using var etoBitmap = new Bitmap(new BitmapHandler(memoryImage.ToBitmapSource()));
        clipboard.Image = etoBitmap;
    }

    public override IMemoryImage DrawHourglass(IMemoryImage image)
    {
        var visual = new SWM.DrawingVisual();
        using (var dc = visual.RenderOpen())
        {
            dc.PushOpacity(0.3);
            dc.DrawImage(image.ToDisplayBitmapSource(), new SW.Rect(0, 0, image.Width, image.Height));
            dc.Pop();
            var hourglass = BitmapFrame.Create(new MemoryStream(Icons.hourglass_grey_32), BitmapCreateOptions.None,
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
        ApplyCompactToggleTemplate(native);
        // A control can be measured just before the layout makes it visible, and collapsed elements measure as empty
        bool collapsed = native.Visibility == SW.Visibility.Collapsed;
        if (collapsed) native.Visibility = SW.Visibility.Hidden;
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
            else if (control is Label or LinkButton or CheckBox or RadioButton or Button)
            {
                // With layout rounding, the desired size can be rounded down (e.g. 145.3 -> 145), and Eto.Wpf's text
                // in these controls wraps (and the second line is clipped) if it's even slightly too narrow
                desired.Width += 1;
            }
            return new SizeF((float) Math.Ceiling(desired.Width), (float) Math.Ceiling(desired.Height));
        }
        finally
        {
            if (collapsed) native.Visibility = SW.Visibility.Collapsed;
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
            Content = new FramePanel { HorizontalAlignment = SW.HorizontalAlignment.Left }
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

    public override void InitForm(Window window)
    {
        if (window is IFormBase form)
        {
            form.LayoutController.Container.ToNative().SizeChanged += (_, _) => form.LayoutController.Invalidate();
        }
        var native = window.ToNative();
        // This mirrors the whole window, including the positions from the layout
        native.FlowDirection = CultureInfo.CurrentCulture.TextInfo.IsRightToLeft
            ? SW.FlowDirection.RightToLeft
            : SW.FlowDirection.LeftToRight;
        if (window is Dialog dialog)
        {
            // Like WinForms, use the active window as the owner (so the dialog is centered on it).
            dialog.PreLoad += (_, _) =>
            {
                var activeWindow = Win32.GetActiveWindow();
                dialog.Owner ??= Application.Instance.Windows.FirstOrDefault(x =>
                    x != dialog && x.NativeHandle == activeWindow);
            };
        }
        // WPF can show the window (e.g. when maximized) before its content is rendered, so hide it until then
        var interop = new SW.Interop.WindowInteropHelper(native);
        native.SourceInitialized += (_, _) =>
        {
            WindowCloaking.SetCloaked(interop.Handle, true);
            UndoBlurBehind(interop.Handle);
        };
        native.ContentRendered += (_, _) => WindowCloaking.SetCloaked(interop.Handle, false);
    }

    private static void UndoBlurBehind(IntPtr hwnd)
    {
        // Fix compat between Eto and the WPF Fluent theme for maximized windows
        const uint DWM_BB_ENABLE = 0x1;
        const int DWMWA_TRANSITIONS_FORCEDISABLED = 3;
        var blurBehind = new Win32.DwmBlurBehind { dwFlags = DWM_BB_ENABLE, fEnable = false };
        Win32.DwmEnableBlurBehindWindow(hwnd, ref blurBehind);
        int transitionsDisabled = 0;
        Win32.DwmSetWindowAttribute(hwnd, DWMWA_TRANSITIONS_FORCEDISABLED, ref transitionsDisabled, sizeof(int));
    }

    public override Size GetClientSize(Window window, bool excludeToolbars = false)
    {
        if (excludeToolbars && window is IFormBase form && form.LayoutController.Container.ToNative() is
                { IsLoaded: true, ActualWidth: > 0, ActualHeight: > 0 } container)
        {
            return new Size((int) container.ActualWidth, (int) container.ActualHeight);
        }
        if (!excludeToolbars && window.ToNative() is { IsLoaded: true, Content: SW.FrameworkElement content })
        {
            // Eto's ClientSize excludes toolbars, so we use the window content (which includes them)
            return new Size((int) content.ActualWidth, (int) content.ActualHeight);
        }
        return base.GetClientSize(window, excludeToolbars);
    }

    public override void SetClientSize(Window window, Size clientSize)
    {
        var native = window.ToNative();
        if (native.Content is not SW.FrameworkElement content)
        {
            base.SetClientSize(window, clientSize);
            return;
        }
        if (native.IsLoaded)
        {
            native.SizeToContent = SW.SizeToContent.Manual;
            native.Width = clientSize.Width + native.ActualWidth - content.ActualWidth;
            native.Height = clientSize.Height + native.ActualHeight - content.ActualHeight;
            return;
        }
        // Before the window is loaded, it sizes itself to fit its content. Constraining the content (rather than
        // using Eto's ClientSize) means toolbars are included in the size and don't widen the window.
        content.MinWidth = content.MaxWidth = clientSize.Width;
        content.MinHeight = content.MaxHeight = clientSize.Height;
        native.Loaded += OnLoaded;

        void OnLoaded(object? sender, SW.RoutedEventArgs e)
        {
            native.Loaded -= OnLoaded;
            native.SizeToContent = SW.SizeToContent.Manual;
            content.MinWidth = content.MinHeight = 0;
            content.MaxWidth = content.MaxHeight = double.PositiveInfinity;
        }
    }

    public override float GetScaleFactor(Window? window)
    {
        var native = window?.ToNative();
        return native == null ? 1f : (float) SWM.VisualTreeHelper.GetDpi(native).DpiScaleX;
    }

    public override void SetImageSize(ImageView imageView, int size)
    {
        // Icons are rendered at the display scale, so they need a fixed logical size or they'd show at their pixel
        // size in DIPs (e.g. double-size at 200%)
        imageView.Size = new Size(size, size);
    }

    public override void AttachDpiDependency(Control control, Action<float> callback)
    {
        var native = control is Window window ? window.ToNative() : control.ToNative();

        void Update() => callback((float) SWM.VisualTreeHelper.GetDpi(native).DpiScaleX);
        void OnDpiChanged(object? sender, SW.DpiChangedEventArgs e) => Update();

        Update();
        native.Loaded += (_, _) =>
        {
            Update();
            if (SW.Window.GetWindow(native) is { } nativeWindow)
            {
                nativeWindow.DpiChanged -= OnDpiChanged;
                nativeWindow.DpiChanged += OnDpiChanged;
            }
        };
    }

    public override void HandleKeyDown(Control control, Func<Keys, bool> handle)
    {
        void OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var key = e.Key == Key.System ? e.SystemKey : e.Key;
            e.Handled = handle(key.ToEtoWithModifier(Keyboard.Modifiers));
        }

        if (control is Window window)
        {
            window.ToNative().KeyDown += OnKeyDown;
        }
        else
        {
            // Use the preview event so controls don't handle keys first
            control.ToNative().PreviewKeyDown += OnKeyDown;
        }
    }

    public override void AttachMouseWheelEvent(Control control, EventHandler<MouseEventArgs> eventHandler)
    {
        var native = control.ToNative();
        // Use the preview event so the ListBox's ScrollViewer doesn't swallow it first
        native.PreviewMouseWheel += (sender, e) =>
        {
            var args = e.ToEto(native);
            eventHandler(sender, args);
            e.Handled = args.Handled;
        };
    }

    public override void AttachMouseMoveEvent(Control control, EventHandler<MouseEventArgs> eventHandler)
    {
        var native = control.ToNative();
        native.MouseMove += (sender, e) => eventHandler(sender, e.ToEto(native));
    }
}
