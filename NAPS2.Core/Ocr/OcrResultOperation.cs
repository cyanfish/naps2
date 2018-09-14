using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NAPS2.Lang.Resources;
using NAPS2.Operation;

namespace NAPS2.Ocr
{
    public class OcrResultOperation : OperationBase
    {
        private readonly List<Task> workerTasks;

        public OcrResultOperation(List<Task> workerTasks)
        {
            this.workerTasks = workerTasks;
            ProgressTitle = MiscResources.OcrProgress;
            AllowBackground = true;
            AllowCancel = true;
            SkipExitPrompt = true;
            Status = new OperationStatus
            {
                StatusText = MiscResources.RunningOcr
            };
        }

        public override void Wait()
        {
            Task.WaitAll(workerTasks.ToArray());
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
