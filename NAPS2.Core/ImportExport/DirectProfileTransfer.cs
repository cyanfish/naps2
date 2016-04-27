using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NAPS2.Scan;

namespace NAPS2.ImportExport
{
    [Serializable]
    public class DirectProfileTransfer
    {
        public DirectProfileTransfer(ScanProfile profile)
        {
            ProcessID = Process.GetCurrentProcess().Id;
            ScanProfile = profile.Clone();

            Locked = ScanProfile.IsLocked;

            ScanProfile.IsDefault = false;
            ScanProfile.IsLocked = false;
            ScanProfile.IsDeviceLocked = false;
        }

        public int ProcessID { get; private set; }

        public ScanProfile ScanProfile { get; private set; }

        public bool Locked { get; private set; }
    }
}
