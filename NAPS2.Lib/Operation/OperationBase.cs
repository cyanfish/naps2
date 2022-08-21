using System.Threading;

namespace NAPS2.Operation;

/// <summary>
/// A base implementation for IOperation, helping with common event logic.
/// </summary>
public abstract class OperationBase : IOperation
{
    private readonly CancellationTokenSource _cts = new();
    private readonly TaskCompletionSource<bool> _tcs = new();

    public string? ProgressTitle { get; protected set; }

    public bool AllowCancel { get; protected set; }

    public bool AllowBackground { get; protected set; }

    public bool SkipExitPrompt { get; protected set; }

    public OperationStatus Status { get; protected set; } = new OperationStatus();

    public Task<bool> Success => _tcs.Task;

    public bool IsFinished { get; protected set; }

    public virtual void Wait(CancellationToken cancelToken = default)
    {
        Success.Wait(cancelToken);
    }

    public virtual void Cancel()
    {
        _cts.Cancel();
    }

    public event EventHandler? StatusChanged;

    public event EventHandler? Finished;

    public event EventHandler<OperationErrorEventArgs>? Error;

    protected OperationErrorEventArgs? LastError { get; private set; }

    protected CancellationToken CancelToken => _cts.Token;

    protected void RunAsync(Func<Task<bool>> action)
    {
        StartTask(action);
    }

    protected void RunAsync(Func<bool> action)
    {
        StartTask(() => Task.FromResult(action()));
    }

    private void StartTask(Func<Task<bool>> action)
    {
        Task.Run(async () =>
        {
            // We don't need to catch errors in general. The idea is that for a typical operation,
            // OperationManager will handle it and show an error message box.
            // For other uses, consumers should catch the errors.
            try
            {
                return await action();
            }
            finally
            {
                InvokeFinished();
            }
            // TODO: Maybe try and move away from "return false on cancel" and use cancellation tokens/OperationCancelledException via ct.ThrowIfCancellationRequested
        }, CancelToken).ContinueWith(t =>
        {
            if (t.IsCanceled)
            {
                _tcs.TrySetCanceled();
            }
            if (t.IsFaulted)
            {
                _tcs.TrySetException(t.Exception!);
            }
            if (t.IsCompleted)
            {
                _tcs.TrySetResult(t.Result);
            }
        }).AssertNoAwait();
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