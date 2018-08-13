using System;
using System.Collections.Generic;
using System.Linq;

#if !WINDOWS

namespace NAPS2.Scan.Wia
{
    public class WiaState
    {
        public WiaState(object device, object item)
        {
            Item = item;
            Device = device;
        }

        public object Device { get; }

        public object Item { get; }
    }
}

#endif
