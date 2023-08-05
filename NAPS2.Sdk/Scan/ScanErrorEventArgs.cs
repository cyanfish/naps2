namespace NAPS2.Scan;

public class ScanErrorEventArgs : EventArgs
{
    public ScanErrorEventArgs(Exception exception)
    {
        Exception = exception;
    }

    /// <summary>
    /// Gets the exception that occurred.
    /// </summary>
    public Exception Exception { get; }
}