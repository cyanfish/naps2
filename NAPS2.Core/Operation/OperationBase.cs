using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Operation
{
    public abstract class OperationBase : IOperation
    {
        public string ProgressTitle { get; protected set; }

        public bool AllowCancel { get; protected set; }

        public OperationStatus Status { get; protected set; }

        public abstract void Cancel();

        public event EventHandler StatusChanged;

        public event EventHandler Finished;

        public event EventHandler<OperationErrorEventArgs> Error;

        protected OperationErrorEventArgs LastError { get; private set; }

        protected void InvokeFinished()
        {
            if (Finished != null)
            {
                Finished.Invoke(this, new EventArgs());
            }
        }

        protected void InvokeStatusChanged()
        {
            if (StatusChanged != null)
            {
                StatusChanged.Invoke(this, new EventArgs());
            }
        }

        protected void InvokeError(string message, Exception exception)
        {
            var args = new OperationErrorEventArgs(message, exception);
            LastError = args;
            if (Error != null)
            {
                Error.Invoke(this, args);
            }
        }
    }
}