using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NAPS2.Scan.Wia.Native
{
    public class WiaDevice : WiaItemBase, IWiaDeviceProps
    {
        protected internal WiaDevice(WiaVersion version, IntPtr handle) : base(version, handle)
        {
        }
        
        public WiaItem PromptToConfigure(IntPtr parentWindowHandle)
        {
            int itemCount = 0;
            IntPtr[] items = new IntPtr[5];
            int fileCount = 0;
            string[] filePaths = new string[0];
            var hr = Version == WiaVersion.Wia10
                ? NativeWiaMethods.ConfigureDevice1(Handle, parentWindowHandle, 0, 0, ref itemCount, ref items)
                : NativeWiaMethods.ConfigureDevice2(Handle, 0, parentWindowHandle, Paths.Temp, Path.GetRandomFileName(), ref fileCount, ref filePaths, items);
            if (hr == 1)
            {
                return null;
            }
            WiaException.Check(hr);
            return new WiaItem(Version, items[0]);
        }
    }
}
