using NAPS2.Scan.Exceptions;
using NAPS2.Wia;

namespace NAPS2.Scan.Internal.Wia;

public class WiaScanErrors
{
    public static void ThrowDeviceError(WiaException e)
    {
        if (e.ErrorCode == WiaErrorCodes.NO_DEVICE_AVAILABLE)
        {
            throw new DeviceNotFoundException();
        }
        if (e.ErrorCode == WiaErrorCodes.PAPER_EMPTY)
        {
            throw new NoPagesException();
        }
        if (e.ErrorCode == WiaErrorCodes.OFFLINE)
        {
            throw new DeviceException(SdkResources.DeviceOffline);
        }
        if (e.ErrorCode == WiaErrorCodes.BUSY)
        {
            throw new DeviceException(SdkResources.DeviceBusy);
        }
        if (e.ErrorCode == WiaErrorCodes.COVER_OPEN)
        {
            throw new DeviceException(SdkResources.DeviceCoverOpen);
        }
        if (e.ErrorCode == WiaErrorCodes.PAPER_JAM)
        {
            throw new DeviceException(SdkResources.DevicePaperJam);
        }
        if (e.ErrorCode == WiaErrorCodes.WARMING_UP)
        {
            throw new DeviceException(SdkResources.DeviceWarmingUp);
        }
        throw new ScanDriverUnknownException(e);
    }
}