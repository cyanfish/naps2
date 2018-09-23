using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAPS2.Scan.Images;
using NAPS2.Util;

namespace NAPS2.Scan
{
    /// <summary>
    /// A high-level interface used for scanning.
    /// This abstracts away the logic of obtaining and using an instance of IScanDriver.
    /// </summary>
    public interface IScanPerformer
    {
        Task PerformScan(ScanProfile scanProfile, ScanParams scanParams, IWin32Window dialogParent, ISaveNotify notify, Action<ScannedImage> imageCallback,
            CancellationToken cancelToken = default);
    }
}
