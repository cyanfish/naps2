namespace NAPS2.Scan.Exceptions;

public class DeviceNotFoundException : ScanDriverException
{
    public DeviceNotFoundException()
        : base(SdkResources.DeviceNotFound)
    {
    }

    public DeviceNotFoundException(string message)
        : base(message)
    {
    }

    public DeviceNotFoundException(Exception innerException)
        : base(SdkResources.DeviceNotFound, innerException)
    {
    }

    public DeviceNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}