namespace NAPS2.Scan.Exceptions;

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