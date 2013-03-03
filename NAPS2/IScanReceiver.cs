using System.Collections.Generic;
using NAPS2.Scan;

namespace NAPS2
{
    public interface IScanReceiver
    {
        void ReceiveScan(IEnumerable<IScannedImage> scannedImages);
    }
}
