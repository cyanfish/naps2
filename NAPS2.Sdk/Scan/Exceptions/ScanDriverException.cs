namespace NAPS2.Scan.Exceptions;

/// <summary>
/// Indicates an exception with the scanning driver. The concrete class may be a well-known exception type (e.g.
/// DeviceOfflineException) or an unknown exception (ScanDriverUnknownException).
/// </summary>
public abstract class ScanDriverException : Exception
{
    protected ScanDriverException()
    {
    }

    protected ScanDriverException(string message)
        : base(message)
    {
    }

    protected ScanDriverException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}