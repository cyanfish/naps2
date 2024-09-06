using CoreAnimation;
using Eto.Drawing;
using Eto.Mac;
using Eto.Mac.Forms.Controls;

namespace NAPS2.EtoForms.Mac;

public class ListViewItem : NSCollectionViewItem
{
    private readonly Image? _itemImage;
    private readonly string? _label;
    private readonly bool _checkbox;
    private readonly Action<bool>? _checkedChanged;
    private readonly Action _onActivate;
    private bool _selected;
    private readonly ColorScheme _colorScheme;
    private NSImageView? _imageView;

    public ListViewItem(Image? itemImage, string? label, bool checkbox, ColorScheme colorScheme,
        Action<bool>? checkedChanged, bool selected, Action onActivate)
    {
        _itemImage = itemImage;
        _label = label;
        _checkbox = checkbox;
        _checkedChanged = checkedChanged;
        _selected = selected;
        _colorScheme = colorScheme;
        _onActivate = onActivate;
    }

    public override void LoadView()
    {
        // TODO: Add padding/insets for the image from its border? Ideally the border shouldn't cover the actual image
        // The Photos app also interestingly has a 1px white between the image and the blue selection border
        // Though we're doing it differently as we have the black border always
        if (_checkbox)
        {
            // Note we can't use NSButton.CreateCheckbox due to https://github.com/xamarin/xamarin-macios/issues/17635
            var button = new NSButton
            {
                Title = _label!,
                State = _selected ? NSCellStateValue.On : NSCellStateValue.Off
            };
            button.SetButtonType(NSButtonType.Switch);
            button.WithAction(() => _checkedChanged!(button.State == NSCellStateValue.On));
            View = button;
        }
        else if (_label != null)
        {
            var image = _itemImage.ToNS();
            // Resize high-dpi images so they render correctly on retina displays
            image.Size = new CGSize(image.Size.Width / 2, image.Size.Height / 2);
            _imageView = new NSImageView
            {
                Image = image
            };
            var stack = NSStackView.FromViews(new NSView[]
            {
                _imageView,
                new EtoLabel
                {
                    StringValue = _label,
                    PreferredMaxLayoutWidth = 90,
                    Alignment = NSTextAlignment.Center
                }
            });
            stack.SetHuggingPriority(500, NSLayoutConstraintOrientation.Horizontal);
            stack.EdgeInsets = new NSEdgeInsets(4, 4, 4, 4);
            stack.Orientation = NSUserInterfaceLayoutOrientation.Vertical;
            stack.Alignment = NSLayoutAttribute.CenterX;
            View = stack;
        }
        else
        {
            _imageView = new NSImageView
            {
                WantsLayer = true,
                Layer = new CALayer
                {
                    MasksToBounds = true,
                    Contents = _itemImage.ToCG()
                }
            };
            View = _imageView;
        }
        View.WantsLayer = true;
        UpdateViewForSelectedState();

        if (!_checkbox)
        {
            View.AddGestureRecognizer(new NSClickGestureRecognizer(_onActivate)
            {
                NumberOfClicksRequired = 2,
                DelaysPrimaryMouseButtonEvents = false
            });
        }
    }

    public override bool Selected
    {
        get => _selected;
        set
        {
            _selected = value;
            UpdateViewForSelectedState();
        }
    }

    private void UpdateViewForSelectedState()
    {
        if (_checkbox) return;
        var layer = View.Layer!;
        layer.BorderWidth = Selected ? 3 : _label == null ? 1 : 0;
        layer.CornerRadius = _label == null ? 0 : 4;
        layer.BorderColor = Selected ? NSColor.SelectedContentBackground.ToCG() : _colorScheme.BorderColor.ToCG();
    }
}