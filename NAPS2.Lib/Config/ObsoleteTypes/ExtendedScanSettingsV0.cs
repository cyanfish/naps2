using System.Xml.Serialization;
using NAPS2.Scan;

namespace NAPS2.Config.ObsoleteTypes;

/// <summary>
/// Used for compatibility when reading old profiles.xml files.
/// </summary>
[XmlType("ExtendedScanSettings")]
public class ExtendedScanSettingsV0 : ScanSettingsV0
{
    public ExtendedScanSettingsV0()
    {
        // Set defaults
        BitDepth = ScanBitDepth.C24Bit;
        PageAlign = ScanHorizontalAlign.Left;
        PageSize = ScanPageSize.Letter;
        Resolution.Dpi = 200;
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

    public PageDimensions? CustomPageSize { get; set; }

    public ScanResolution Resolution { get; set; } = new();

    public ScanSource PaperSource { get; set; }
}