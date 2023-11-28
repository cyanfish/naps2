using Eto.Drawing;
using NAPS2.Remoting.Server;

namespace NAPS2.EtoForms.Widgets;

public class SharedDevicesListViewBehavior : ListViewBehavior<SharedDevice>
{
    public SharedDevicesListViewBehavior(ColorScheme colorScheme) : base(colorScheme)
    {
        MultiSelect = false;
        ShowLabels = true;
        ScrollOnDrag = false;
    }

    public override string GetLabel(SharedDevice item) => item.Name;

    public override Image GetImage(SharedDevice item, int imageSize) => Icons.scanner_wireless.ToEtoImage();
}