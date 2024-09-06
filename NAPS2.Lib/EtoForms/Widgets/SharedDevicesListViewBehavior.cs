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

    public override Image GetImage(IListView<SharedDevice> listView, SharedDevice item)
    {
        var scale = EtoPlatform.Current.GetScaleFactor(listView.Control.ParentWindow);
        return EtoPlatform.Current.IconProvider.GetIcon("scanner_wireless_48", scale)!;
    }
}