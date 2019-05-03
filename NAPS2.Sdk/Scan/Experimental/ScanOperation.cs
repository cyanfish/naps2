using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NAPS2.Lang.Resources;
using NAPS2.Operation;

namespace NAPS2.Scan.Experimental
{
    public class ScanOperation : OperationBase
    {
        private int pageNumber = 1;

        public ScanOperation(ScanDevice device, PaperSource paperSource)
        {
            ProgressTitle = device.Name;
            Status = new OperationStatus
            {
                StatusText = paperSource == PaperSource.Flatbed
                    ? MiscResources.AcquiringData
                    : string.Format(MiscResources.ScanProgressPage, pageNumber),
                MaxProgress = 1000,
                ProgressType = OperationProgressType.BarOnly
            };
            AllowBackground = true;
            AllowCancel = true;
        }

        public new CancellationToken CancelToken => base.CancelToken;

        public void Progress(int current, int total)
        {
            Status.CurrentProgress = current;
            Status.MaxProgress = total;
            InvokeStatusChanged();
        }

        public void NextPage(int newPageNumber)
        {
            pageNumber = newPageNumber;
            Status.StatusText = string.Format(MiscResources.ScanProgressPage, pageNumber);
            InvokeStatusChanged();
        }
    }
}