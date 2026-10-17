namespace NAPS2.Operation;

/// <summary>
/// Arguments for the IOperation.Error event.
/// </summary>
public class OperationErrorEventArgs : EventArgs
{
    public OperationErrorEventArgs(string errorMessage, Exception exception, bool canRetry = false)
    {
        ErrorMessage = errorMessage;
        Exception = exception;
        CanRetry = canRetry;
    }

    public string ErrorMessage { get; }

    public Exception Exception { get; }

    /// <summary>
    /// Whether the operation can retry the failed action. If so, an event handler can set RetryTask.
    /// </summary>
    public bool CanRetry { get; }

    /// <summary>
    /// Set by an event handler when CanRetry is true. Resolves to true if the operation should retry.
    /// </summary>
    public Task<bool> RetryTask { get; set; } = Task.FromResult(false);
}