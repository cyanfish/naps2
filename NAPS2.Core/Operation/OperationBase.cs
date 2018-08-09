using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Operation
{
    /// <summary>
    /// A base implementation for IOperation, helping with common event logic.
    /// </summary>
    public abstract class OperationBase : IOperation
    {
        protected volatile bool cancel;

        public string ProgressTitle { get; protected set; }

        public bool AllowCancel { get; protected set; }

        public OperationStatus Status { get; protected set; }

        public void Cancel()
        {
            cancel = true;
        }

        public abstract void WaitUntilFinished();

        public event EventHandler StatusChanged;

        public event EventHandler Finished;

        public event EventHandler<OperationErrorEventArgs> Error;

        protected OperationErrorEventArgs LastError { get; private set; }

        protected void InvokeFinished()
        {
            Finished?.Invoke(this, new EventArgs());
        }

        protected void InvokeStatusChanged()
        {
            StatusChanged?.Invoke(this, new EventArgs());
        }

        protected void InvokeError(string message, Exception exception)
        {
            var args = new OperationErrorEventArgs(message, exception);
            LastError = args;
            Error?.Invoke(this, args);
        }

        protected virtual bool OnProgress(int current, int max)
        {
            // TODO: Maybe don't make this virtual. Instead, clone the status object, and project event invocations back to the client.
            Status.CurrentProgress = current;
            Status.MaxProgress = max;
            InvokeStatusChanged();
            return !cancel;
        }
    }
}