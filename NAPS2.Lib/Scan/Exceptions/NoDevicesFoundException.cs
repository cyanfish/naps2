namespace NAPS2.Scan.Exceptions;

public class NoDevicesFoundException : ScanDriverException
{
    public NoDevicesFoundException()
        : base(SdkResources.NoDevicesFound)
    {
    }

    public NoDevicesFoundException(string message)
        : base(message)
    {
    }

    public NoDevicesFoundException(Exception innerException)
        : base(SdkResources.NoDevicesFound, innerException)
    {
    }

    public NoDevicesFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}