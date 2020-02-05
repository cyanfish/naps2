using System;

namespace NAPS2.Wia
{
    public class WiaDevice : WiaItemBase, IWiaDeviceProps
    {
        protected internal WiaDevice(WiaVersion version, IntPtr handle) : base(version, handle)
        {
        }
        
        public WiaItem? PromptToConfigure(IntPtr parentWindowHandle)
        {
            if (Version == WiaVersion.Wia20)
            {
                throw new InvalidOperationException("WIA 2.0 does not support PromptToConfigure. Use WiaDeviceManager.PromptForImage if you want to use the native WIA 2.0 UI.");
            }

            int itemCount = 0;
            IntPtr[]? items = null;
            var hr = NativeWiaMethods.ConfigureDevice1(Handle, parentWindowHandle, 0, 0, ref itemCount, ref items);
            if (hr == 1)
            {
                return null;
            }
            WiaException.Check(hr);
            if (items == null)
            {
                return null;
            }
            return new WiaItem(Version, items[0]);
        }
    }
}
