namespace NAPS2.Scan.Exceptions;

/// <summary>
/// Indicates an exception related to the physical device (e.g. offline, busy, paper jam).
/// </summary>
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

/// <summary>
/// Indicates the scanning device's automatic document feeder (ADF) is empty and doesn't have any pages to scan.
/// </summary>
public class DeviceFeederEmptyException() : DeviceException(SdkResources.NoPagesInFeeder);

/// <summary>
/// Indicates the scanning device is offline and a connection couldn't be established.
/// </summary>
public class DeviceOfflineException() : DeviceException(SdkResources.DeviceOffline);

/// <summary>
/// Indicates the connection with the scanning device was interrupted.
/// </summary>
public class DeviceCommunicationException() : DeviceException(SdkResources.DeviceCommunicationFailure);

/// <summary>
/// Indicates the scanning device is busy with another operation. Maybe retry later.
/// </summary>
public class DeviceBusyException() : DeviceException(SdkResources.DeviceBusy);

/// <summary>
/// Indicates the scanning device is currently warming up and can't be interacted with.
/// </summary>
public class DeviceWarmingUpException() : DeviceException(SdkResources.DeviceWarmingUp);

/// <summary>
/// Indicates the scanning device's flatbed cover is open and needs to be closed before scanning.
/// </summary>
public class DeviceCoverOpenException() : DeviceException(SdkResources.DeviceCoverOpen);

/// <summary>
/// Indicates the scanning device could not be found. It may have been uninstalled or disconnected.
/// </summary>
public class DeviceNotFoundException() : DeviceException(SdkResources.DeviceNotFound);

/// <summary>
/// Indicates the scanning device's automatic document feeder (ADF) has a paper jam that needs to be cleared.
/// </summary>
public class DevicePaperJamException() : DeviceException(SdkResources.DevicePaperJam);