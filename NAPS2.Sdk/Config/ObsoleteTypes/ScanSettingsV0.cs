using System.Xml.Serialization;

namespace NAPS2.Config.ObsoleteTypes
{
    /// <summary>
    /// Used for compatibility when reading old profiles.xml files.
    /// </summary>
    [XmlInclude(typeof(ExtendedScanSettingsV0))]
    [XmlType("ScanSettings")]
    public class ScanSettingsV0
    {
        public ScanDeviceV0? Device { get; set; }

        public string? DriverName { get; set; }

        public string? DisplayName { get; set; }

        public int IconID { get; set; }

        public bool MaxQuality { get; set; }

        public bool IsDefault { get; set; }
    }
}
