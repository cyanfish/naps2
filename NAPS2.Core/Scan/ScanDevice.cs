using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Scan
{
    /// <summary>
    /// The representation of a scanning device identified by a driver.
    /// </summary>
    [Serializable]
    public class ScanDevice
    {
        public ScanDevice(string id, string name)
        {
            ID = id;
            Name = name;
        }

        public ScanDevice()
        {
        }

        public string ID { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// This property only exists for compatibility when reading profiles.xml from an older version. Use ScanProfile.DriverName instead.
        /// </summary>
        public string DriverName { get; set; }
    }
}
