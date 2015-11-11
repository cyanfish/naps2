using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NAPS2.Lang.Resources;
using NAPS2.Util;

namespace NAPS2.Scan.Batch
{
    public class BatchScanPerformer
    {
        public BatchScanPerformer()
        {
        }

        public void PerformBatchScan(BatchSettings settings, Func<string, bool> progressCallback)
        {
            if (!progressCallback(string.Format(MiscResources.BatchStatusPage, "1")))
            {
                return;
            }
            Thread.Sleep(2000);
            if (!progressCallback(string.Format(MiscResources.BatchStatusPage, "2")))
            {
                return;
            }
            Thread.Sleep(2000);
            if (!progressCallback(string.Format(MiscResources.BatchStatusWaitingForScan, "2")))
            {
                return;
            }
            Thread.Sleep(2000);
            return;
        }
    }
}
