using Eto.Drawing;
using NAPS2.Scan;

namespace NAPS2.EtoForms.Widgets;

public class DeviceListViewBehavior : ListViewBehavior<ScanDevice>
{
    private readonly Dictionary<ScanDevice, Image> _imageMap = new();

    public DeviceListViewBehavior(ColorScheme colorScheme) : base(colorScheme)
    {
        MultiSelect = false;
        ShowLabels = true;
        ScrollOnDrag = false;
    }

    public void SetImage(ScanDevice item, Image image) => _imageMap[item] = image;

    public override string GetLabel(ScanDevice item) => item.Name;

    public override Image GetImage(IListView<ScanDevice> listView, ScanDevice item)
    {
        return (_imageMap.Get(item)?.Clone() ?? Icons.device.ToEtoImage()).PadTo(listView.ImageSize);
    }
}