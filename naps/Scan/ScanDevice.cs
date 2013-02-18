using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NAPS2.Scan
{
    public class ScanDevice : IScanDevice
    {
        public ScanDevice(string id, string name)
        {
            ID = id;
            Name = name;
        }

        public string ID { get; private set; }
        public string Name { get; private set; }
    }
}
