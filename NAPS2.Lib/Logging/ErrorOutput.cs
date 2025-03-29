namespace NAPS2.Logging;

/// <summary>
/// A base interface for objects capable of displaying error output.
/// </summary>
public abstract class ErrorOutput
{
    public abstract void DisplayError(string errorMessage);

    public abstract void DisplayError(string errorMessage, string details, string? link = null);

    public abstract void DisplayError(string errorMessage, Exception exception, string? link = null);
}