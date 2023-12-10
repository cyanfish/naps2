namespace NAPS2.Scan;

public class ScanEndEventArgs : EventArgs
{
    public ScanEndEventArgs(Exception? error = null)
    {
        Error = error;
    }

    /// <summary>
    /// Gets the exception that ended the scan, if any.
    /// </summary>
    public Exception? Error { get; }

    public bool HasError => Error != null;
}