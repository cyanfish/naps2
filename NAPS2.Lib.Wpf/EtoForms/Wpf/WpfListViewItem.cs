using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Eto.Wpf;
using NAPS2.EtoForms.Widgets;
using Image = System.Windows.Controls.Image;

namespace NAPS2.EtoForms.Wpf;

/// <summary>
/// An item in a WpfListView, showing an image, potentially with a frame/label.
/// </summary>
internal class WpfListViewItem<T> : ListBoxItem where T : notnull
{
    private const int PAGE_NUMBER_PADDING = 4;

    // Width of items with labels (e.g. profiles), so labels can be wider than the icon
    private const int LABELED_ITEM_WIDTH = 80;

    private const int BORDER_THICKNESS = 1;
    private const int SELECTED_BORDER_THICKNESS = 3;
    private static readonly Brush SelectedBorderBrush =
        (Brush) new SolidColorBrush(Color.FromRgb(0x60, 0xa0, 0xe8)).GetAsFrozen();

    private readonly ListViewBehavior<T> _behavior;
    private readonly Image _image = new() { Stretch = Stretch.Fill };
    private readonly Border _imageBorder;
    private readonly Grid _imageHost;
    private readonly TextBlock _label;

    public WpfListViewItem(ListViewBehavior<T> behavior, T value)
    {
        SetResourceReference(StyleProperty, typeof(ListBoxItem));
        _behavior = behavior;
        Value = value;
        RenderOptions.SetBitmapScalingMode(_image, BitmapScalingMode.HighQuality);
        _imageBorder = new Border
        {
            Child = _image,
            BorderThickness = new Thickness(behavior.ShowLabels ? 0 : BORDER_THICKNESS),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        _imageHost = new Grid { Children = { _imageBorder } };
        _label = new TextBlock
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            TextWrapping = behavior.ShowLabels ? TextWrapping.Wrap : TextWrapping.NoWrap,
            Margin = new Thickness(0, PAGE_NUMBER_PADDING, 0, 0)
        };
        Cursor = behavior.UseHandCursor ? Cursors.Hand : null;
        if (behavior.Checkboxes)
        {
            // The selection is shown as the checkbox state
            Template = XamlTemplates.CheckBoxItemTemplate;
            Content = behavior.GetLabel(value);
            return;
        }
        // The label goes at the bottom, with the image centered in the remaining space
        DockPanel.SetDock(_label, Dock.Bottom);
        Content = new DockPanel { Children = { _label, _imageHost } };
        // Keyboard focus is already clear enough from the selection
        FocusVisualStyle = null;
        if (behavior.ShowLabels)
        {
            _label.Text = behavior.GetLabel(value);
            _label.FontSize = 12;
            Padding = new Thickness(4);
            VerticalAlignment = VerticalAlignment.Top;
        }
        else
        {
            Template = XamlTemplates.ThumbnailItemTemplate;
            BorderThickness = new Thickness(1);
            Selected += (_, _) => UpdateVisuals();
            Unselected += (_, _) => UpdateVisuals();
        }
        Update();
    }

    public void Update()
    {
        _label.Visibility = _behavior.ShowLabels || _behavior.ShowPageNumbers ? Visibility.Visible : Visibility.Collapsed;
        if (!_behavior.ShowLabels)
        {
            // With page numbers, the selection box always fills the vertical space; otherwise we just center
            VerticalAlignment = _behavior.ShowPageNumbers ? VerticalAlignment.Stretch : VerticalAlignment.Center;
        }
        UpdateVisuals();
    }

    public T Value { get; set; }

    public void SetImage(ImageSource source, Eto.Drawing.Size imageSize)
    {
        _image.Source = source;
        UpdateSize(imageSize);
    }

    public void SetPageNumber(int number, int count) => _label.Text = $"{number} / {count}";

    public void UpdateSize(Eto.Drawing.Size imageSize)
    {
        _imageHost.Width = imageSize.Width;
        _imageHost.Height = imageSize.Height;
        if (_behavior.ShowLabels)
        {
            _label.Width = Math.Max(imageSize.Width, LABELED_ITEM_WIDTH);
        }
        else
        {
            _label.MaxWidth = imageSize.Width;
        }
        if (_image.Source is not { } source || source.Width == 0 || source.Height == 0) return;
        // Fit the image within the thumbnail size, preserving the aspect ratio
        double scale = Math.Min(imageSize.Width / source.Width, imageSize.Height / source.Height);
        _image.Width = Math.Round(source.Width * scale);
        _image.Height = Math.Round(source.Height * scale);
        if (_behavior.ShowLabels) return;
        // Shrink thumbnails around the image (plus its border)
        _imageHost.Width = _image.Width + 2 * BORDER_THICKNESS;
        _imageHost.Height = _image.Height + 2 * BORDER_THICKNESS;
        _label.MaxWidth = Math.Max(_imageHost.Width, imageSize.Width / 2.0);
    }

    public void UpdateVisuals()
    {
        if (_behavior.ShowLabels) return;
        var colors = _behavior.ColorScheme;
        var imageBorderBrush = (SolidColorBrush) Application.Current.FindResource("ControlStrongStrokeColorDefaultBrush")!;
        if (_behavior.ShowPageNumbers)
        {
            // Highlight the whole item (including the page number), with no image border
            Background = IsSelected ? colors.HighlightBackgroundColor.ToWpfBrush() : Brushes.Transparent;
            BorderBrush = IsSelected ? colors.HighlightBorderColor.ToWpfBrush() : Brushes.Transparent;
            _imageBorder.BorderBrush = IsSelected ? Brushes.Transparent : imageBorderBrush;
            _imageBorder.BorderThickness = new Thickness(BORDER_THICKNESS);
            _imageBorder.Margin = new Thickness(0);
            return;
        }
        Background = Brushes.Transparent;
        BorderBrush = Brushes.Transparent;
        // Grow the border outwards so the image doesn't move
        int extra = SELECTED_BORDER_THICKNESS - BORDER_THICKNESS;
        _imageBorder.BorderBrush = IsSelected ? SelectedBorderBrush : imageBorderBrush;
        _imageBorder.BorderThickness = new Thickness(IsSelected ? SELECTED_BORDER_THICKNESS : BORDER_THICKNESS);
        _imageBorder.Margin = new Thickness(IsSelected ? -extra : 0);
    }
}
