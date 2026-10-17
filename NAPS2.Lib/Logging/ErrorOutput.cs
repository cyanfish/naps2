namespace NAPS2.Logging;

/// <summary>
/// A base interface for objects capable of displaying error output.
/// </summary>
public abstract class ErrorOutput
{
    public abstract void DisplayError(string errorMessage);

    public abstract void DisplayError(string errorMessage, string details);

    public abstract void DisplayError(string errorMessage, Exception exception);

    /// <summary>
    /// Displays the error with an option for the user to retry. The returned task resolves to true if the user chose
    /// to retry. By default, this displays the error normally and doesn't retry.
    /// </summary>
    public virtual Task<bool> DisplayErrorWithRetry(string errorMessage, Exception exception)
    {
        DisplayError(errorMessage, exception);
        return Task.FromResult(false);
    }
}