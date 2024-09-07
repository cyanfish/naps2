using Eto.Drawing;
using NAPS2.Scan;

namespace NAPS2.EtoForms.Widgets;

public class DeviceListViewBehavior : ListViewBehavior<ScanDevice>
{
    private readonly Dictionary<ScanDevice, Image> _imageMap = new();
    private readonly Dictionary<ScanDevice, string> _iconNameMap = new();

    public DeviceListViewBehavior(ColorScheme colorScheme) : base(colorScheme)
    {
        MultiSelect = false;
        ShowLabels = true;
        ScrollOnDrag = false;
    }

    public void SetImage(ScanDevice item, Image image) => _imageMap[item] = image;

    public void SetIconName(ScanDevice item, string iconName) => _iconNameMap[item] = iconName;

    public override string GetLabel(ScanDevice item) => item.Name;

    public override Image GetImage(IListView<ScanDevice> listView, ScanDevice item)
    {
        float scale = EtoPlatform.Current.GetScaleFactor(listView.Control.ParentWindow);
        if (_imageMap.Get(item) is { } image)
        {
            int scaledSize = (int) Math.Round(48 * scale);
            return image.Clone().ResizeTo(scaledSize).PadTo(Size.Round(listView.ImageSize * scale));
        }
        string iconName = _iconNameMap.Get(item) ?? "device";
        return EtoPlatform.Current.IconProvider.GetIcon(iconName, scale)!.PadTo(Size.Round(listView.ImageSize * scale));
    }
}