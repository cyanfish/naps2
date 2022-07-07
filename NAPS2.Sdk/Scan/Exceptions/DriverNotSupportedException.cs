namespace NAPS2.Scan.Exceptions;

public class DriverNotSupportedException : ScanDriverException
{
    public DriverNotSupportedException()
        : base(SdkResources.DriverNotSupported)
    {
    }

    public DriverNotSupportedException(string message)
        : base(message)
    {
    }

    public DriverNotSupportedException(Exception innerException)
        : base(SdkResources.DriverNotSupported, innerException)
    {
    }

    public DriverNotSupportedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}