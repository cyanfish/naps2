namespace NAPS2.Scan.Exceptions;

/// <summary>
/// Indicates that a driver was selected that isn't supported with the current platform or framework.
/// </summary>
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