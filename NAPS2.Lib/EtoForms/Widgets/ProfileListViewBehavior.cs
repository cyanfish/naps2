using Eto.Drawing;
using Eto.Forms;
using NAPS2.ImportExport.Profiles;
using NAPS2.Scan;

namespace NAPS2.EtoForms.Widgets;

public class ProfileListViewBehavior : ListViewBehavior<ScanProfile>
{
    private readonly ProfileTransfer _profileTransfer = new();

    public ProfileListViewBehavior(ColorScheme colorScheme) : base(colorScheme)
    {
        MultiSelect = false;
        ShowLabels = true;
        ScrollOnDrag = false;
    }

    public bool NoUserProfiles { get; set; }

    public override string GetLabel(ScanProfile item) => item.DisplayName ?? "";

    public override Image GetImage(ScanProfile item, Size imageSize)
    {
        var iconName = (item.IsDefault, item.IsLocked) switch
        {
            (true, true) => "scanner_lock_default_48",
            (true, false) => "scanner_default_48",
            (false, true) => "scanner_lock_48",
            (false, false) => "scanner_48"
        };
        var scale = imageSize.Height / 48f;
        return EtoPlatform.Current.IconProvider.GetIcon(iconName, scale)!;
    }

    public override bool AllowDragDrop => true;

    public override string CustomDragDataType => _profileTransfer.TypeName;

    public override DragEffects GetCustomDragEffect(byte[] data)
    {
        if (NoUserProfiles)
        {
            return DragEffects.None;
        }
        var dataObj = _profileTransfer.FromBinaryData(data);
        return dataObj.ProcessId == Process.GetCurrentProcess().Id
            ? dataObj.Locked
                ? DragEffects.None
                : DragEffects.Move
            : DragEffects.Copy;
    }

    public override byte[] SerializeCustomDragData(ScanProfile[] items)
    {
        return _profileTransfer.ToBinaryData(items.Single());
    }
}