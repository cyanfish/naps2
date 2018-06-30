﻿using NAPS2.Scan;
using System;
using System.Diagnostics;

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

        public int ProcessID { get; }

        public ScanProfile ScanProfile { get; }

        public bool Locked { get; }
    }
}