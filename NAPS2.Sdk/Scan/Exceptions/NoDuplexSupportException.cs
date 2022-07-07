namespace NAPS2.Scan.Exceptions;

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