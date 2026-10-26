using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
        new(ButtonPadding: 10, ContentMinWidth: 56, ArrowWidth: 22, StackedPadding: 8, VerticalPadding: 6),
        new(ButtonPadding: 8, ContentMinWidth: 52, ArrowWidth: 21, StackedPadding: 7, VerticalPadding: 4),
        new(ButtonPadding: 7, ContentMinWidth: 48, ArrowWidth: 21, StackedPadding: 6, VerticalPadding: 2),
        new(ButtonPadding: 5, ContentMinWidth: 44, ArrowWidth: 20, StackedPadding: 5, VerticalPadding: 1),
        new(ButtonPadding: 4, ContentMinWidth: 40, ArrowWidth: 19, StackedPadding: 4, VerticalPadding: 0)
    ];

    // The padding that isn't compacted, i.e. horizontal padding for a vertical toolbar and vice versa
    private const int FIXED_PADDING = 4;

    private readonly List<(Button button, FrameworkElement content, bool isSplit)> _largeButtons = [];
    private readonly List<Button> _arrowButtons = [];
    private readonly List<Button> _stackedButtons = [];
    private int _density = -1;
    private bool _vertical;

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
        var button = StyleButton(new Button
        {
            Content = content,
            // Centers the content when the button is stretched in a vertical toolbar
            HorizontalContentAlignment = HorizontalAlignment.Center
        });
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
        var contentGrid = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition { Width = GridLength.Auto, SharedSizeGroup = "StackedContent" } },
            Children = { content }
        };
        var button = StyleButton(new Button
        {
            Content = contentGrid,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center
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
        bool vertical = toolBar.Orientation == Orientation.Vertical;
        if (vertical != _vertical)
        {
            _vertical = vertical;
            _density = -1;
            PlaceOverflowButton(toolBar);
        }
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
        // Only compact along the toolbar's orientation, as that's where it might not fit
        int h = _vertical ? FIXED_PADDING : d.ButtonPadding;
        int v = _vertical ? d.VerticalPadding : FIXED_PADDING;
        foreach (var (button, content, isSplit) in _largeButtons)
        {
            // In a vertical toolbar, split buttons get extra left padding to balance the arrow, so their content is
            // centered in the toolbar like other buttons
            button.Padding = isSplit
                ? new Thickness(_vertical ? d.ArrowWidth + 2 : h, v, 2, v)
                : new Thickness(h, v, h, v);
            content.MinWidth = d.ContentMinWidth;
        }
        foreach (var arrow in _arrowButtons)
        {
            arrow.Width = d.ArrowWidth;
        }
        foreach (var button in _stackedButtons)
        {
            button.Padding = _vertical
                ? new Thickness(FIXED_PADDING * 2, FIXED_PADDING, FIXED_PADDING * 2, FIXED_PADDING)
                : new Thickness(d.StackedPadding, FIXED_PADDING, d.StackedPadding, FIXED_PADDING);
        }
    }

    private void PlaceOverflowButton(ToolBar toolBar)
    {
        var overflowButton = toolBar.FindDescendant<ToggleButton>(x => x.TemplatedParent == toolBar);
        if (overflowButton != null)
        {
            DockPanel.SetDock(overflowButton, _vertical ? Dock.Bottom : Dock.Right);
            if (overflowButton.FindDescendant<TextBlock>(x => x.TemplatedParent == overflowButton) is { } arrow)
            {
                if (_vertical)
                    arrow.HorizontalAlignment = HorizontalAlignment.Center;
                else
                    arrow.ClearValue(FrameworkElement.HorizontalAlignmentProperty);
            }
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

    private record ToolbarDensity(
        int ButtonPadding, int ContentMinWidth, int ArrowWidth, int StackedPadding, int VerticalPadding);
}