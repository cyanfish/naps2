using CoreAnimation;
using Eto.Drawing;
using Eto.Mac;

namespace NAPS2.EtoForms.Mac;

public class ListViewItem : NSCollectionViewItem
{
    private readonly Image _itemImage;
    private bool _selected;

    public ListViewItem(Image itemImage)
    {
        _itemImage = itemImage;
    }

    public override void LoadView()
    {
        // TODO: Add padding/insets for the image from its border? Ideally the border shouldn't cover the actual image
        // The Photos app also interestingly has a 1px white between the image and the blue selection border
        // Though we're doing it differently as we have the black border always
        View = new NSImageView
        {
            WantsLayer = true,
            Layer = new CALayer
            {
                // TODO: Rounded corners are an option but it feels wrong, at least for the normal black border
                MasksToBounds = true,
                Contents = _itemImage.ToCG()
            }
        };
        UpdateViewForSelectedState();
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
        var layer = ((NSImageView) View).Layer!;
        layer.BorderWidth = Selected ? 3 : 1;
        layer.BorderColor = Selected ? NSColor.SelectedContentBackground.ToCG() : NSColor.Black.ToCG();
    }
}