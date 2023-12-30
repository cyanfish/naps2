namespace NAPS2.Scan.Exceptions;

/// <summary>
/// Indicates that PaperSource.Duplex was selected but the scanning device/driver doesn't support duplex scanning.
/// </summary>
public class NoDuplexSupportException : ScanDriverException
{
    public NoDuplexSupportException()
        : base(SdkResources.NoDuplexSupport)
    {
    }

    public NoDuplexSupportException(string message)
        : base(message)
    {
    }

    public NoDuplexSupportException(Exception innerException)
        : base(SdkResources.NoDuplexSupport, innerException)
    {
    }

    public NoDuplexSupportException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}