using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Automation;
using System.Windows.Input;
using System.Windows.Interop;
using Eto.Forms;
using NAPS2.Platform.Windows;
using SW = System.Windows;
using SWC = System.Windows.Controls;
using SWM = System.Windows.Media;

namespace NAPS2.EtoForms.Wpf;

/// <summary>
/// A message dialog styled like a WinUI ContentDialog using the WPF Fluent theme resources.
/// </summary>
public class FluentMessageDialog : Dialog<DialogResult>
{
    private const double MAX_WIDTH = 548;
    private const double PADDING = 24;
    private const double FOOTER_VERTICAL_PADDING = 14;
    private const double BUTTON_MIN_WIDTH = 80;
    private const double ICON_SIZE = 32;
    private const double ICON_MARGIN = 16;

    private readonly DialogResult? _cancelResult;
    private bool _buttonClicked;

    public record Button(string Text, DialogResult Result);

    public FluentMessageDialog(string caption, string text, MessageBoxType type, IReadOnlyList<Button> buttons,
        DialogResult defaultResult, DialogResult? cancelResult)
    {
        _cancelResult = cancelResult;
        Title = caption;
        Resizable = false;
        Minimizable = false;
        Maximizable = false;
        ShowInTaskbar = false;
        Closeable = cancelResult != null;
        Result = cancelResult ?? defaultResult;

        var icon = CreateIcon(type);
        var textBlock = new SWC.TextBlock
        {
            Text = text,
            FontSize = 14,
            TextWrapping = SW.TextWrapping.Wrap,
            HorizontalAlignment = SW.HorizontalAlignment.Left,
            VerticalAlignment = SW.VerticalAlignment.Center
        };
        var (footer, defaultButton) = CreateFooter(buttons, defaultResult);
        var root = new SWC.Grid
        {
            MinWidth = 320,
            MaxWidth = MAX_WIDTH,
            RowDefinitions = { new SWC.RowDefinition(), new SWC.RowDefinition { Height = SW.GridLength.Auto } },
            Children = { CreateContent(icon, textBlock), footer }
        };
        SWC.Grid.SetRow(footer, 1);
        root.SetResourceReference(SWC.Panel.BackgroundProperty, "SolidBackgroundFillColorBaseBrush");
        root.SetResourceReference(SWC.TextBlock.ForegroundProperty, "TextFillColorPrimaryBrush");
        Content = root.ToEto();

        var native = this.ToNative();
        native.SizeToContent = SW.SizeToContent.WidthAndHeight;
        native.SourceInitialized += (_, _) => SetDialogFrame(new WindowInteropHelper(native).Handle);
        native.Loaded += (_, _) =>
        {
            // SizeToContent measures the text on one line, but layout rounding can leave it slightly less room so
            // it wraps and gets clipped; a fixed width keeps the measured and arranged layouts the same
            textBlock.Measure(new SW.Size(double.PositiveInfinity, double.PositiveInfinity));
            double maxTextWidth = MAX_WIDTH - 2 * PADDING - (icon != null ? ICON_SIZE + ICON_MARGIN : 0);
            textBlock.Width = Math.Min(Math.Ceiling(textBlock.DesiredSize.Width) + 1, maxTextWidth);
            defaultButton.Focus();
        };
        native.KeyDown += (_, e) =>
        {
            if (e.Key == Key.C && System.Windows.Input.Keyboard.Modifiers == ModifierKeys.Control)
            {
                SW.Clipboard.SetText($"{Title}\n\n{text}");
            }
        };
        EtoPlatform.Current.InitForm(this);
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);
        if (_cancelResult == null && !_buttonClicked)
        {
            e.Cancel = true;
        }
    }

    private static SWC.Border CreateContent(SW.FrameworkElement? icon, SWC.TextBlock textBlock)
    {
        var grid = new SWC.Grid
        {
            ColumnDefinitions =
            {
                new SWC.ColumnDefinition { Width = SW.GridLength.Auto },
                new SWC.ColumnDefinition()
            }
        };
        if (icon != null)
        {
            grid.Children.Add(icon);
        }
        SWC.Grid.SetColumn(textBlock, 1);
        grid.Children.Add(textBlock);
        var content = new SWC.Border { Padding = new SW.Thickness(PADDING), Child = grid };
        content.SetResourceReference(SWC.Border.BackgroundProperty, "LayerFillColorAltBrush");
        return content;
    }

    private (SWC.Border footer, SWC.Button defaultButton) CreateFooter(IReadOnlyList<Button> buttons,
        DialogResult defaultResult)
    {
        var grid = new SWC.Grid { HorizontalAlignment = SW.HorizontalAlignment.Right };
        SWC.Grid.SetIsSharedSizeScope(grid, true);
        SWC.Button? defaultButton = null;
        for (int i = 0; i < buttons.Count; i++)
        {
            if (i > 0) grid.ColumnDefinitions.Add(new SWC.ColumnDefinition { Width = new SW.GridLength(8) });
            grid.ColumnDefinitions.Add(new SWC.ColumnDefinition
                { Width = SW.GridLength.Auto, SharedSizeGroup = "Buttons" });

            var result = buttons[i].Result;
            var button = new SWC.Button
            {
                Content = new SWC.AccessText { Text = buttons[i].Text },
                MinWidth = BUTTON_MIN_WIDTH,
                Padding = new SW.Thickness(11, 3, 11, 4),
                IsDefault = result == defaultResult,
                IsCancel = result == _cancelResult
            };
            AutomationProperties.SetName(button, Regex.Replace(buttons[i].Text, "_(.)", "$1"));
            if (result == defaultResult)
            {
                button.SetResourceReference(SW.FrameworkElement.StyleProperty, "AccentButtonStyle");
                defaultButton = button;
            }
            button.Click += (_, _) =>
            {
                _buttonClicked = true;
                Close(result);
            };
            SWC.Grid.SetColumn(button, i * 2);
            grid.Children.Add(button);
        }
        var footer = new SWC.Border
        {
            Padding = new SW.Thickness(PADDING, FOOTER_VERTICAL_PADDING, PADDING, FOOTER_VERTICAL_PADDING),
            BorderThickness = new SW.Thickness(0, 1, 0, 0),
            Child = grid
        };
        footer.SetResourceReference(SWC.Border.BorderBrushProperty, "CardStrokeColorDefaultBrush");
        return (footer, defaultButton ?? (SWC.Button) grid.Children[0]);
    }

    private static SW.FrameworkElement? CreateIcon(MessageBoxType type)
    {
        (string shape, string glyph, object background, object foreground)? icon = type switch
        {
            MessageBoxType.Warning => (FluentIcons.StatusTriangleOuter, FluentIcons.StatusTriangleExclamation,
                WarningBackground, WarningForeground),
            MessageBoxType.Error => (FluentIcons.StatusCircleOuter, FluentIcons.StatusCircleErrorX,
                "SystemFillColorCriticalBrush", "TextFillColorInverseBrush"),
            MessageBoxType.Information => (FluentIcons.StatusCircleOuter, FluentIcons.StatusCircleInfo,
                "AccentFillColorDefaultBrush", "TextFillColorInverseBrush"),
            MessageBoxType.Question => (FluentIcons.StatusCircleOuter, FluentIcons.StatusCircleQuestionMark,
                "AccentFillColorDefaultBrush", "TextFillColorInverseBrush"),
            _ => null
        };
        if (icon is not var (shape, glyph, background, foreground)) return null;
        // The high contrast severity and inverse text brushes are the same color
        if (SW.SystemParameters.HighContrast)
        {
            background = SW.SystemColors.HighlightBrushKey;
            foreground = SW.SystemColors.HighlightTextBrushKey;
        }
        return new SWC.Grid
        {
            Margin = new SW.Thickness(0, 0, ICON_MARGIN, 0),
            VerticalAlignment = SW.VerticalAlignment.Top,
            Children = { CreateGlyph(shape, background), CreateGlyph(glyph, foreground) }
        };
    }

    private static readonly SWM.Brush WarningBackground = Frozen(new SWM.SolidColorBrush(SWM.Color.FromRgb(0xFF, 0xB9, 0x00)));
    private static readonly SWM.Brush WarningForeground = Frozen(new SWM.SolidColorBrush(SWM.Color.FromArgb(0xE4, 0, 0, 0)));

    private static SWM.Brush Frozen(SWM.Brush brush)
    {
        brush.Freeze();
        return brush;
    }

    private static SWC.TextBlock CreateGlyph(string glyph, object brush)
    {
        var textBlock = new SWC.TextBlock
        {
            Text = glyph,
            FontFamily = FluentIcons.FontFamily,
            FontSize = ICON_SIZE
        };
        if (brush is SWM.Brush fixedBrush)
        {
            textBlock.Foreground = fixedBrush;
        }
        else
        {
            textBlock.SetResourceReference(SWC.TextBlock.ForegroundProperty, brush);
        }
        return textBlock;
    }

    private static void SetDialogFrame(IntPtr hwnd)
    {
        // No icon in the title bar, like the native message box
        Win32.SetWindowLong(hwnd, Win32.GWL_EXSTYLE,
            Win32.GetWindowLong(hwnd, Win32.GWL_EXSTYLE) | Win32.WS_EX_DLGMODALFRAME);
        Win32.SendMessage(hwnd, Win32.WM_SETICON, IntPtr.Zero, IntPtr.Zero);
        Win32.SendMessage(hwnd, Win32.WM_SETICON, new IntPtr(1), IntPtr.Zero);
        Win32.SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0,
            Win32.SWP_NOSIZE | Win32.SWP_NOMOVE | Win32.SWP_NOZORDER | Win32.SWP_NOACTIVATE |
            Win32.SWP_FRAMECHANGED);
    }
}
