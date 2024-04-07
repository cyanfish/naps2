#if !MAC
using NAPS2.Scan.Exceptions;
using NAPS2.Wia;

namespace NAPS2.Scan.Internal.Wia;

internal class WiaScanErrors
{
    public static void ThrowDeviceError(WiaException e)
    {
        throw e.ErrorCode switch
        {
            WiaErrorCodes.NO_DEVICE_AVAILABLE => new DeviceNotFoundException(),
            WiaErrorCodes.PAPER_EMPTY => new DeviceFeederEmptyException(),
            WiaErrorCodes.OFFLINE => new DeviceOfflineException(),
            WiaErrorCodes.COMMUNICATION => new DeviceCommunicationException(),
            WiaErrorCodes.BUSY => new DeviceBusyException(),
            WiaErrorCodes.COVER_OPEN => new DeviceCoverOpenException(),
            WiaErrorCodes.PAPER_JAM => new DevicePaperJamException(),
            WiaErrorCodes.WARMING_UP => new DeviceWarmingUpException(),
            _ => new ScanDriverUnknownException(e)
        };
    }
}
#endif