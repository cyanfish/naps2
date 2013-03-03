using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Scan;

namespace NAPS2
{
    public interface IScanReceiver
    {
        void ReceiveScan(IEnumerable<IScannedImage> scannedImages);
    }
}
