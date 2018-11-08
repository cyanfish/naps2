using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Scan.Wia.Native
{
    public class WiaDeviceInfo : NativeWiaObject, IWiaDeviceProps
    {
        protected internal WiaDeviceInfo(IntPtr propStorageHandle)
        {
            Properties = new WiaPropertyCollection(propStorageHandle);
        }

        public WiaPropertyCollection Properties { get; }
    }
}
