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

    protected ProgressHandler ProgressHandler => new(OnProgress, CancelToken);

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

    protected void FailedToStart()
    {
        InvokeFinished();
        _tcs.TrySetResult(false);
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

    /// <summary>
    /// Reports an error with an option for the user to retry. Returns true if the operation should retry.
    /// </summary>
    protected async Task<bool> InvokeErrorWithRetry(string message, Exception exception)
    {
        var args = new OperationErrorEventArgs(message, exception, true);
        Error?.Invoke(this, args);
        bool retry = await args.RetryTask;
        if (!retry)
        {
            LastError = args;
        }
        return retry;
    }

    /// <summary>
    /// Runs the action, and if it throws, reports the error with an option for the user to retry. Returns the result
    /// of the action, or false if it failed and the user didn't retry.
    /// </summary>
    protected async Task<bool> RunWithRetry(Func<Task<bool>> action, Func<Exception, string> getErrorMessage)
    {
        while (true)
        {
            try
            {
                return await action();
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (Exception ex)
            {
                if (CancelToken.IsCancellationRequested)
                {
                    return false;
                }
                var message = getErrorMessage(ex);
                Log.ErrorException(message, ex);
                if (!await InvokeErrorWithRetry(message, ex))
                {
                    return false;
                }
            }
        }
    }

    /// <summary>
    /// Waits for the file to not be in use (if it exists), reporting an error with an option for the user to retry
    /// while it is. Returns false if the user didn't retry.
    /// </summary>
    protected async Task<bool> WaitForFileNotInUse(string filePath)
    {
        while (FileSystemHelper.IsFileInUse(filePath, out var ex))
        {
            if (!await InvokeErrorWithRetry(MiscResources.FileInUse, ex!))
            {
                return false;
            }
        }
        return true;
    }

    protected static string GetSaveErrorMessage(Exception ex) =>
        ex is UnauthorizedAccessException ? MiscResources.DontHavePermission : MiscResources.ErrorSaving;

    protected void OnProgress(int current, int max)
    {
        Status.CurrentProgress = current;
        Status.MaxProgress = max;
        InvokeStatusChanged();
    }
}