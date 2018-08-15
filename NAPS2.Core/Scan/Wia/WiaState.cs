// See WiaApi.cs for an explanation of this guard
#if !NONWINDOWS

using System;
using System.Collections.Generic;
using System.Linq;
using WIA;

namespace NAPS2.Scan.Wia
{
    public class WiaState
    {
        public WiaState(Device device, Item item)
        {
            Item = item;
            Device = device;
        }

        public Device Device { get; }

        public Item Item { get; }
    }
}

#endif
