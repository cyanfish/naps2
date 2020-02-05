using System;
using System.Collections.Generic;
using System.IO;

namespace NAPS2.Wia
{
    public class WiaDeviceManager : NativeWiaObject
    {
        private const int SCANNER_DEVICE_TYPE = 1;
        private const int SELECT_DEVICE_NODEFAULT = 1;

        public WiaDeviceManager() : base(WiaVersion.Default)
        {
        }

        public WiaDeviceManager(WiaVersion version) : base(version)
        {
            WiaException.Check(Version == WiaVersion.Wia10
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

        public WiaDevice? PromptForDevice(IntPtr parentWindowHandle)
        {
            var hr = Version == WiaVersion.Wia10
                ? NativeWiaMethods.SelectDevice1(Handle, parentWindowHandle, SCANNER_DEVICE_TYPE, SELECT_DEVICE_NODEFAULT, out _, out var deviceHandle)
                : NativeWiaMethods.SelectDevice2(Handle, parentWindowHandle, SCANNER_DEVICE_TYPE, SELECT_DEVICE_NODEFAULT, out _, out deviceHandle);
            if (hr == 1)
            {
                return null;
            }
            WiaException.Check(hr);
            return new WiaDevice(Version, deviceHandle); ;
        }

        public string[]? PromptForImage(IntPtr parentWindowHandle, WiaDevice device, string? tempFolder = null)
        {
            tempFolder ??= Path.GetTempPath();
            var fileName = Path.GetRandomFileName();
            IntPtr itemHandle = IntPtr.Zero;
            int fileCount = 0;
            string[] filePaths = new string[0];
            var hr = Version == WiaVersion.Wia10
                ? NativeWiaMethods.GetImage1(Handle, parentWindowHandle, SCANNER_DEVICE_TYPE, 0, 0, Path.Combine(tempFolder, fileName), IntPtr.Zero)
                : NativeWiaMethods.GetImage2(Handle, 0, device.Id(), parentWindowHandle, tempFolder, fileName, ref fileCount, ref filePaths, ref itemHandle);
            if (hr == 1)
            {
                return null;
            }
            WiaException.Check(hr);
            return filePaths ?? new[] { Path.Combine(tempFolder, fileName) };
        }
    }
}
