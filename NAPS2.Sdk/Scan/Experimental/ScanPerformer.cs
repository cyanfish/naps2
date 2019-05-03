using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NAPS2.Images;
using NAPS2.Lang.Resources;
using NAPS2.Operation;
using NAPS2.Util;

namespace NAPS2.Scan.Experimental
{
    public class ScanPerformer : IScanPerformer
    {
        public ScannedImageSource PerformScan(ScanProfile scanProfile, ScanParams scanParams, IntPtr dialogParent = default,
            CancellationToken cancelToken = default)
        {
            var options = BuildOptions(scanProfile, scanParams, dialogParent);
            var controller = new ScanController();
            var op = new ScanOperation(options.Device, options.PaperSource);

            controller.PageStart += (sender, args) => op.NextPage(args.PageNumber);
            TranslateProgress(controller, op);
            cancelToken.Register(op.Cancel);

            return controller.Scan(options, op.CancelToken);
        }

        private void TranslateProgress(ScanController controller, ScanOperation op)
        {
            var smoothProgress = new SmoothProgress();
            controller.PageStart += (sender, args) => smoothProgress.Reset();
            controller.PageProgress += (sender, args) => smoothProgress.InputProgressChanged(args.Progress);
            smoothProgress.OutputProgressChanged += (sender, args) => op.Progress((int) Math.Round(args.Value * 1000), 1000);
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

            public void NextPage(int newPageNumber)
            {
                pageNumber = newPageNumber;
                Status.StatusText = string.Format(MiscResources.ScanProgressPage, pageNumber);
                InvokeStatusChanged();
            }
        }
    }
}
