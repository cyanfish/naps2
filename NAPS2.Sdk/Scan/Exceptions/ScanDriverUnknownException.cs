namespace NAPS2.Scan.Exceptions;

/// <summary>
/// Indicates an unknown exception with the scanning driver that should be logged with full diagnostics.
/// </summary>
public class ScanDriverUnknownException : ScanDriverException
{
    public ScanDriverUnknownException()
    {
    }

    public ScanDriverUnknownException(Exception innerException)
        : base(SdkResources.UnknownDriverError, innerException)
    {
    }

    public ScanDriverUnknownException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}