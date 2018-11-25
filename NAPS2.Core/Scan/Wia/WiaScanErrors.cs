using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Lang.Resources;
using NAPS2.Scan.Exceptions;
using NAPS2.Scan.Wia.Native;

namespace NAPS2.Scan.Wia
{
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
                throw new DeviceException(MiscResources.DeviceOffline);
            }
            if (e.ErrorCode == WiaErrorCodes.BUSY)
            {
                throw new DeviceException(MiscResources.DeviceBusy);
            }
            if (e.ErrorCode == WiaErrorCodes.COVER_OPEN)
            {
                throw new DeviceException(MiscResources.DeviceCoverOpen);
            }
            if (e.ErrorCode == WiaErrorCodes.PAPER_JAM)
            {
                throw new DeviceException(MiscResources.DevicePaperJam);
            }
            if (e.ErrorCode == WiaErrorCodes.WARMING_UP)
            {
                throw new DeviceException(MiscResources.DeviceWarmingUp);
            }
            throw new ScanDriverUnknownException(e);
        }
    }
}