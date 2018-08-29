using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace NAPS2.Scan
{
    /// <summary>
    /// Used for compatibility when reading old profiles.xml files.
    /// </summary>
    [XmlInclude(typeof(OldExtendedScanSettings))]
    [XmlType("ScanSettings")]
    public class OldScanSettings
    {
        public ScanDevice Device { get; set; }

        public string DriverName { get; set; }

        public string DisplayName { get; set; }

        public int IconID { get; set; }

        public bool MaxQuality { get; set; }

        public bool IsDefault { get; set; }
    }

    /// <summary>
    /// Used for compatibility when reading old profiles.xml files.
    /// </summary>
    [XmlType("ExtendedScanSettings")]
    public class OldExtendedScanSettings : OldScanSettings
    {
        public OldExtendedScanSettings()
        {
            // Set defaults
            BitDepth = ScanBitDepth.C24Bit;
            PageAlign = ScanHorizontalAlign.Left;
            PageSize = ScanPageSize.Letter;
            Resolution = ScanDpi.Dpi200;
            PaperSource = ScanSource.Glass;
        }

        public int Version { get; set; }

        public bool UseNativeUI { get; set; }

        public ScanScale AfterScanScale { get; set; }

        public int Brightness { get; set; }

        public int Contrast { get; set; }

        public ScanBitDepth BitDepth { get; set; }

        public ScanHorizontalAlign PageAlign { get; set; }

        public ScanPageSize PageSize { get; set; }

        public PageDimensions CustomPageSize { get; set; }

        public ScanDpi Resolution { get; set; }

        public ScanSource PaperSource { get; set; }
    }
}
