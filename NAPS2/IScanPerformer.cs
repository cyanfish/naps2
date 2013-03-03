using NAPS2.Scan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NAPS2
{
    public interface IScanPerformer
    {
        void PerformScan(ScanSettings scanSettings, IWin32Window dialogParent, IScanReceiver scanReceiver);
    }
}
