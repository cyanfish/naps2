using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace NAPS2.Config.ObsoleteTypes
{
    /// <summary>
    /// Used for compatibility when reading old profiles.xml files.
    /// </summary>
    [XmlType("ScanDevice")]
    public class ScanDeviceV0
    {
        public string ID { get; set; }

        public string Name { get; set; }

        public string DriverName { get; set; }
    }
}
