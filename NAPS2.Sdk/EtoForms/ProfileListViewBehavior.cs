using System;
using System.Diagnostics;
using System.Linq;
using Eto.Drawing;
using Eto.Forms;
using Eto.WinForms;
using NAPS2.Images;
using NAPS2.ImportExport;
using NAPS2.Logging;
using NAPS2.Scan;

namespace NAPS2.EtoForms
{
    public class ProfileListViewBehavior : ListViewBehavior<ScanProfile>
    {
        private ProfileTransfer _profileTransfer;

        public ProfileListViewBehavior(ProfileTransfer profileTransfer)
        {
            _profileTransfer = profileTransfer;
            MultiSelect = false;
            ShowLabels = true;
        }

        public override string GetLabel(ScanProfile item) => item.DisplayName ?? "";

        public override Image GetImage(ScanProfile item)
        {
            if (item.IsDefault && item.IsLocked)
            {
                return Icons.scanner_lock_default.ToEto();
            }
            if (item.IsDefault)
            {
                return Icons.scanner_default.ToEto();
            }
            if (item.IsLocked)
            {
                return Icons.scanner_lock.ToEto();
            }
            return Icons.scanner_48.ToEto();
        }

        public override void SetDragData(ListSelection<ScanProfile> selection, IDataObject dataObject)
        {
            if (selection.Count > 0)
            {
                _profileTransfer.AddTo(dataObject, selection.Single());
            }
        }

        public override DragEffects GetDropEffect(IDataObject dataObject)
        {
            // Determine if drop data is compatible
            try
            {
                if (_profileTransfer.IsIn(dataObject))
                {
                    var data = _profileTransfer.GetFrom(dataObject);
                    return data.ProcessId == Process.GetCurrentProcess().Id
                        ? data.Locked
                            ? DragEffects.None
                            : DragEffects.Move
                        : DragEffects.Copy;
                }
            }
            catch (Exception ex)
            {
                Log.ErrorException("Error receiving drag/drop", ex);
            }
            return DragEffects.None;
        }
    }
}
