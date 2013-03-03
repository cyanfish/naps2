using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NAPS2.Scan;

namespace NAPS2
{
    public interface IScanPerformer
    {
        void PerformScan(ScanSettings scanSettings, IWin32Window dialogParent, IScanReceiver scanReceiver);
    }
}
