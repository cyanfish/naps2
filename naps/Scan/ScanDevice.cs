using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NAPS2.Scan
{
    public class ScanDevice
    {
        public ScanDevice(string id, string name, string driverName)
        {
            ID = id;
            Name = name;
            DriverName = driverName;
        }

        public ScanDevice()
        {
        }

        public string ID { get; set; }
        public string Name { get; set; }
        public string DriverName { get; set; }
    }
}
