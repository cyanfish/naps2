namespace NAPS2.Scan.Exceptions;

public class NoFeederSupportException : ScanDriverException
{
    public NoFeederSupportException()
        : base(SdkResources.NoFeederSupport)
    {
    }

    public NoFeederSupportException(string message)
        : base(message)
    {
    }

    public NoFeederSupportException(Exception innerException)
        : base(SdkResources.NoFeederSupport, innerException)
    {
    }

    public NoFeederSupportException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}