using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NAPS2.Images;
using NAPS2.Lang.Resources;
using NAPS2.Operation;

namespace NAPS2.Scan.Experimental
{
    public class ScanPerformer : IScanPerformer
    {
        private readonly IScanController scanController;

        public ScanPerformer(IScanController scanController)
        {
            this.scanController = scanController;
        }

        public ScannedImageSource PerformScan(ScanProfile scanProfile, ScanParams scanParams, IntPtr dialogParent = default,
            CancellationToken cancelToken = default)
        {
            var options = BuildOptions(scanProfile, scanParams, dialogParent);
            var op = new ScanOperation(options.Device, options.PaperSource);
            cancelToken.Register(op.Cancel);
            var source = scanController.Scan(options, op.Progress, op.CancelToken);
            // TODO: op.NextPage() when source receives an image
            return source;
        }

        private ScanOptions BuildOptions(ScanProfile scanProfile, ScanParams scanParams, IntPtr dialogParent)
        {
            // TODO: Add device prompting here (including special case for wia)
            // Open question: option validation happens internally, how do we handle Driver.Default?
            // It might make sense to make that public. Double processing is annoying though.
            throw new NotImplementedException();
        }

        private class ScanOperation : OperationBase
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

            public void NextPage()
            {
                pageNumber++;
                Status.StatusText = string.Format(MiscResources.ScanProgressPage, pageNumber);
                InvokeStatusChanged();
            }
        }
    }
}
