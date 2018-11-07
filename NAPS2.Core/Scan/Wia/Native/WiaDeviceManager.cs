using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Scan.Wia.Native
{
    public class WiaDeviceManager : NativeWiaObject
    {
        public WiaDeviceManager()
        {
            WiaException.Check(NativeWiaMethods.GetDeviceManager(out var handle));
            Handle = handle;
        }

        protected internal WiaDeviceManager(IntPtr handle) : base(handle)
        {
        }
        
        public IEnumerable<WiaItem> GetDevices()
        {
            return null;
        }

        public WiaItem FindDevice(string deviceID)
        {
            WiaException.Check(NativeWiaMethods.GetDevice(Handle, deviceID, out var deviceHandle));
            return new WiaItem(deviceHandle);
        }
    }
}
