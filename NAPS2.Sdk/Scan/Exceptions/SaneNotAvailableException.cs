namespace NAPS2.Scan.Exceptions;

public class SaneNotAvailableException : ScanDriverException
{
    private const string PACKAGES = "\nsane\nsane-utils";

    public SaneNotAvailableException() : base(SdkResources.SaneNotAvailable + PACKAGES)
    {
    }

    public SaneNotAvailableException(Exception innerException) : base(SdkResources.SaneNotAvailable + PACKAGES, innerException)
    {
    }
}