using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NAPS2.Util;

namespace NAPS2.Operation
{
    /// <summary>
    /// A base implementation for IOperation, helping with common event logic.
    /// </summary>
    public abstract class OperationBase : IOperation
    {
        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        public string ProgressTitle { get; protected set; }

        public bool AllowCancel { get; protected set; }

        public bool AllowBackground { get; protected set; }

        public bool SkipExitPrompt { get; protected set; }

        public OperationStatus Status { get; protected set; }

        public Task<bool> Success { get; protected set; }

        public bool IsFinished { get; protected set; }

        public virtual void Wait(CancellationToken cancelToken = default)
        {
            Success?.Wait(cancelToken);
        }

        public virtual void Cancel()
        {
            cts.Cancel();
        }

        public event EventHandler StatusChanged;

        public event EventHandler Finished;

        public event EventHandler<OperationErrorEventArgs> Error;

        protected OperationErrorEventArgs LastError { get; private set; }

        protected CancellationToken CancelToken => cts.Token;

        protected void RunAsync(Func<Task<bool>> action)
        {
            Success = StartTask(() => action().Result);
        }

        protected void RunAsync(Func<bool> action)
        {
            Success = StartTask(action);
        }

        private Task<T> StartTask<T>(Func<T> action)
        {
            return Task.Factory.StartNew(() =>
            {
                // We don't need to catch errors in general. The idea is that for a typical operation,
                // OperationManager will handle it and show an error message box.
                // For other uses, consumers should catch the errors.
                try
                {
                    return action();
                }
                finally
                {
                    InvokeFinished();
                }
                // TODO: Maybe try and move away from "return false on cancel" and use cancellation tokens/OperationCancelledException via ct.ThrowIfCancellationRequested
            }, CancelToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        protected void InvokeFinished()
        {
            IsFinished = true;
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

        protected void OnProgress(int current, int max)
        {
            Status.CurrentProgress = current;
            Status.MaxProgress = max;
            InvokeStatusChanged();
        }
    }
}