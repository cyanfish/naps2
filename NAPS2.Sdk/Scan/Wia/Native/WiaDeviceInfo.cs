using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Scan.Wia.Native
{
    public class WiaDeviceInfo : NativeWiaObject, IWiaDeviceProps
    {
        protected internal WiaDeviceInfo(WiaVersion version, IntPtr propStorageHandle) : base(version)
        {
            Properties = new WiaPropertyCollection(version, propStorageHandle);
        }

        public WiaPropertyCollection Properties { get; }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                Properties?.Dispose();
            }
        }
    }
}
