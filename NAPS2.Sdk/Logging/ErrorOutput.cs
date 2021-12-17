namespace NAPS2.Logging;

/// <summary>
/// A base interface for objects capable of displaying error output.
/// </summary>
public abstract class ErrorOutput
{
    public abstract void DisplayError(string errorMessage);

    public abstract void DisplayError(string errorMessage, string details);

    public abstract void DisplayError(string errorMessage, Exception exception);
}