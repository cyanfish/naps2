﻿using WIA;

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