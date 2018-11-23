using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Scan.Wia
{
    public class WiaConfiguration
    {
        public Dictionary<int, object> DeviceProps { get; set; }

        public Dictionary<int, object> ItemProps { get; set; }

        public string ItemName { get; set; }
    }
}
