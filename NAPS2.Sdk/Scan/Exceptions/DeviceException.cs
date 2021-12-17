namespace NAPS2.Scan.Exceptions;

public class DeviceException : ScanDriverException
{
    public DeviceException(string message)
        : base(message)
    {
    }

    public DeviceException()
    {
    }
}