#if !MAC
using NAPS2.Scan.Exceptions;
using NAPS2.Wia;

namespace NAPS2.Scan.Internal.Wia;

public class WiaScanErrors
{
    public static void ThrowDeviceError(WiaException e)
    {
        throw e.ErrorCode switch
        {
            WiaErrorCodes.NO_DEVICE_AVAILABLE => new DeviceNotFoundException(),
            WiaErrorCodes.PAPER_EMPTY => new NoPagesException(),
            WiaErrorCodes.OFFLINE => new DeviceException(SdkResources.DeviceOffline),
            WiaErrorCodes.BUSY => new DeviceException(SdkResources.DeviceBusy),
            WiaErrorCodes.COVER_OPEN => new DeviceException(SdkResources.DeviceCoverOpen),
            WiaErrorCodes.PAPER_JAM => new DeviceException(SdkResources.DevicePaperJam),
            WiaErrorCodes.WARMING_UP => new DeviceException(SdkResources.DeviceWarmingUp),
            _ => new ScanDriverUnknownException(e)
        };
    }
}
#endif