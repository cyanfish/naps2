using Eto.Drawing;
using Eto.Forms;
using NAPS2.ImportExport.Profiles;
using NAPS2.Scan;

namespace NAPS2.EtoForms.Widgets;

public class ProfileListViewBehavior : ListViewBehavior<ScanProfile>
{
    private readonly ProfileTransfer _profileTransfer;

    public ProfileListViewBehavior(ProfileTransfer profileTransfer)
    {
        _profileTransfer = profileTransfer;
        MultiSelect = false;
        ShowLabels = true;
        ScrollOnDrag = false;
    }

    public bool NoUserProfiles { get; set; }

    public override string GetLabel(ScanProfile item) => item.DisplayName ?? "";

    public override Image GetImage(ScanProfile item, int imageSize)
    {
        if (item.IsDefault && item.IsLocked)
        {
            return Icons.scanner_lock_default.ToEtoImage();
        }
        if (item.IsDefault)
        {
            return Icons.scanner_default.ToEtoImage();
        }
        if (item.IsLocked)
        {
            return Icons.scanner_lock.ToEtoImage();
        }
        return Icons.scanner_48_old.ToEtoImage();
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