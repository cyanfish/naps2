using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Platform;

namespace NAPS2.Scan.Wia.Native
{
    public class WiaDeviceManager : NativeWiaObject
    {
        public static WiaVersion DefaultWiaVersion => PlatformCompat.System.IsWia20Supported ? WiaVersion.Wia20 : WiaVersion.Wia10;

        public WiaDeviceManager(WiaVersion? version = null) : base(version ?? DefaultWiaVersion)
        {
            WiaException.Check(version == WiaVersion.Wia10
                ? NativeWiaMethods.GetDeviceManager1(out var handle)
                : NativeWiaMethods.GetDeviceManager2(out handle));
            Handle = handle;
        }

        protected internal WiaDeviceManager(WiaVersion version, IntPtr handle) : base(version, handle)
        {
        }
        
        public IEnumerable<WiaDeviceInfo> GetDeviceInfos()
        {
            List<WiaDeviceInfo> result = new List<WiaDeviceInfo>();
            WiaException.Check(Version == WiaVersion.Wia10
                ? NativeWiaMethods.EnumerateDevices1(Handle, x => result.Add(new WiaDeviceInfo(Version, x)))
                : NativeWiaMethods.EnumerateDevices2(Handle, x => result.Add(new WiaDeviceInfo(Version, x))));
            return result;
        }

        public WiaDevice FindDevice(string deviceID)
        {
            WiaException.Check(Version == WiaVersion.Wia10
                ? NativeWiaMethods.GetDevice1(Handle, deviceID, out var deviceHandle)
                : NativeWiaMethods.GetDevice2(Handle, deviceID, out deviceHandle));
            return new WiaDevice(Version, deviceHandle);
        }
    }
}
