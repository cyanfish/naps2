using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace NAPS2.EtoForms.Wpf;

/// <summary>
/// Creates styled toolbar buttons, and adjusts their spacing to try and avoid or minimize overflow.
/// </summary>
public class WpfToolbarFormatter
{
    // Toolbar text wraps at this width
    private const int MAX_LABEL_WIDTH = 80;

    // Progressively more compact toolbar layouts, used when the toolbar doesn't fit in the window
    private static readonly ToolbarDensity[] Densities =
    [
        new(ButtonPadding: 10, ContentMinWidth: 56, ArrowWidth: 22, StackedPadding: 8),
        new(ButtonPadding: 8, ContentMinWidth: 52, ArrowWidth: 21, StackedPadding: 7),
        new(ButtonPadding: 7, ContentMinWidth: 48, ArrowWidth: 21, StackedPadding: 6),
        new(ButtonPadding: 5, ContentMinWidth: 44, ArrowWidth: 20, StackedPadding: 5),
        new(ButtonPadding: 4, ContentMinWidth: 40, ArrowWidth: 19, StackedPadding: 4)
    ];

    private readonly List<(Button button, FrameworkElement content, bool isSplit)> _largeButtons = [];
    private readonly List<Button> _arrowButtons = [];
    private readonly List<Button> _stackedButtons = [];
    private int _density = -1;

    /// <summary>
    /// Creates the content for a toolbar button: a large icon with text below, or a small icon with text to the right.
    /// The caller is responsible for setting the image source.
    /// </summary>
    public (FrameworkElement content, Image image, TextBlock label) CreateButtonContent(string text, bool large)
    {
        int iconSize = large ? 32 : 16;
        var image = new Image { Width = iconSize, Height = iconSize };
        RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.HighQuality);
        var label = new TextBlock
        {
            Text = text,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            TextWrapping = large ? TextWrapping.Wrap : TextWrapping.NoWrap,
            MaxWidth = large ? MAX_LABEL_WIDTH : double.PositiveInfinity,
            Margin = large ? new Thickness(0, 4, 0, 0) : new Thickness(6, 0, 0, 0)
        };
        var content = new StackPanel
        {
            Orientation = large ? Orientation.Vertical : Orientation.Horizontal,
            MinWidth = large ? Densities[0].ContentMinWidth : 0,
            Children = { image, label }
        };
        return (content, image, label);
    }

    /// <summary>
    /// Creates a button with large content. If it's the main part of a split button, its
    /// right padding is reduced so it sits next to the arrow.
    /// </summary>
    public Button CreateLargeButton(FrameworkElement content, bool isSplit = false)
    {
        var button = StyleButton(new Button { Content = content });
        _largeButtons.Add((button, content, isSplit));
        return button;
    }

    public Button CreateArrowButton()
    {
        var button = StyleButton(new Button
        {
            Content = new TextBlock
            {
                Text = FluentIcons.ChevronDown,
                FontFamily = FluentIcons.FontFamily,
                FontSize = 10
            },
            Padding = new Thickness(0, 4, 0, 4),
            MinWidth = 0,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center
        });
        _arrowButtons.Add(button);
        return button;
    }

    public Button CreateStackedButton(FrameworkElement content)
    {
        var button = StyleButton(new Button
        {
            Content = content,
            // Stretch so both buttons have the same width and their icons line up
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Left
        });
        _stackedButtons.Add(button);
        return button;
    }

    public Button CreateProfileButton(FrameworkElement content) =>
        StyleButton(new Button { Content = content, Padding = new Thickness(6, 4, 6, 4) });

    public void Clear()
    {
        _largeButtons.Clear();
        _arrowButtons.Clear();
        _stackedButtons.Clear();
        _density = -1;
    }

    /// <summary>
    /// Uses the least compact layout that fits in the window. Anything that still doesn't fit goes in the overflow menu
    /// of the ToolBar.
    /// </summary>
    public void RelayoutToolbar(ToolBar toolBar)
    {
        if (!toolBar.IsLoaded) return;
        for (int i = 0; i < Densities.Length; i++)
        {
            ApplyDensity(i);
            toolBar.UpdateLayout();
            if (!toolBar.HasOverflowItems) break;
        }
    }

    private void ApplyDensity(int density)
    {
        if (density == _density) return;
        _density = density;
        var d = Densities[density];
        foreach (var (button, content, isSplit) in _largeButtons)
        {
            button.Padding = new Thickness(d.ButtonPadding, 4, isSplit ? 2 : d.ButtonPadding, 4);
            content.MinWidth = d.ContentMinWidth;
        }
        foreach (var arrow in _arrowButtons)
        {
            arrow.Width = d.ArrowWidth;
        }
        foreach (var button in _stackedButtons)
        {
            button.Padding = new Thickness(d.StackedPadding, 4, d.StackedPadding, 4);
        }
    }

    private static Button StyleButton(Button button)
    {
        button.SetResourceReference(FrameworkElement.StyleProperty, ToolBar.ButtonStyleKey);
        button.Template = XamlTemplates.SubtleButtonTemplate;
        button.FocusVisualStyle = XamlTemplates.InsetFocusVisualStyle;
        // Don't take focus from e.g. the image list when clicked
        button.PreviewGotKeyboardFocus += (_, e) =>
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed || Mouse.RightButton == MouseButtonState.Pressed)
            {
                e.Handled = true;
            }
        };
        return button;
    }

    private record ToolbarDensity(int ButtonPadding, int ContentMinWidth, int ArrowWidth, int StackedPadding);
}