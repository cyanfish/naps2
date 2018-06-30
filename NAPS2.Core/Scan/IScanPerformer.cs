using NAPS2.Scan.Images;
using NAPS2.Util;
using System;
using System.Windows.Forms;

namespace NAPS2.Scan
{
    public interface IScanPerformer
    {
        void PerformScan(ScanProfile scanProfile, ScanParams scanParams, IWin32Window dialogParent, ISaveNotify notify, Action<ScannedImage> imageCallback);
    }
}