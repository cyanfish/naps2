using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NAPS2.Lang.Resources;
using NAPS2.Operation;

namespace NAPS2.Ocr
{
    public class OcrResultOperation : OperationBase
    {
        public OcrResultOperation()
        {
            ProgressTitle = MiscResources.OcrProgress;
            AllowBackground = true;
            AllowCancel = true;
            Status = new OperationStatus
            {
                StatusText = MiscResources.RunningOcr
            };
        }

        public new CancellationToken CancelToken => base.CancelToken;

        public void IncrementMax()
        {
            Status.MaxProgress += 1;
            InvokeStatusChanged();
        }

        public void IncrementCurrent()
        {
            Status.CurrentProgress += 1;
            InvokeStatusChanged();
        }

        public void Finish()
        {
            InvokeFinished();
        }
    }
}
