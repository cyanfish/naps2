namespace NAPS2.Scan.Exceptions;

/// <summary>
/// Indicates an exception that the driver already handled (i.e. gave the user an error message) and doesn't require
/// further notification.
/// </summary>
public class AlreadyHandledDriverException : DeviceException
{
}