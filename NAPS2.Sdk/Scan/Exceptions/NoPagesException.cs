namespace NAPS2.Scan.Exceptions;

public class NoPagesException : ScanDriverException
{
    public NoPagesException()
        : base(SdkResources.NoPagesInFeeder)
    {
    }

    public NoPagesException(string message)
        : base(message)
    {
    }

    public NoPagesException(Exception innerException)
        : base(SdkResources.NoPagesInFeeder, innerException)
    {
    }

    public NoPagesException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}