using System;
using System.Diagnostics;
using NAPS2.Scan;
using NAPS2.Serialization;

namespace NAPS2.ImportExport
{
    [Serializable]
    public class DirectProfileTransfer
    {
        public DirectProfileTransfer(ScanProfile profile)
        {
            ProcessID = Process.GetCurrentProcess().Id;
            ScanProfileXml = profile.ToXml();

            Locked = profile.IsLocked;

            profile.IsDefault = false;
            profile.IsLocked = false;
            profile.IsDeviceLocked = false;
        }

        public int ProcessID { get; }

        public string ScanProfileXml { get; }

        public bool Locked { get; }
    }
}
