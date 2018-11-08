using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Scan.Wia.Native
{
    public class WiaDevice : WiaItemBase, IWiaDeviceProps
    {
        protected internal WiaDevice(WiaVersion version, IntPtr handle) : base(version, handle)
        {
        }
    }
}
