using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NAPS2.Lang.Resources;
using NAPS2.Operation;

namespace NAPS2.Ocr
{
    public class OcrOperation : OperationBase
    {
        private readonly List<Task> workerTasks;

        public OcrOperation(List<Task> workerTasks)
        {
            this.workerTasks = workerTasks;
            ProgressTitle = MiscResources.OcrProgress;
            AllowBackground = true;
            AllowCancel = true;
            SkipExitPrompt = true;
            Status = new OperationStatus
            {
                StatusText = MiscResources.RunningOcr,
                IndeterminateProgress = true
            };
        }

        public override void Wait()
        {
            Task.WaitAll(workerTasks.ToArray());
        }

        public new CancellationToken CancelToken => base.CancelToken;

        public new void InvokeStatusChanged() => base.InvokeStatusChanged();

        public new void InvokeFinished() => base.InvokeFinished();
    }
}
