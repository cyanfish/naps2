using NAPS2.Scan;

namespace NAPS2.Recovery;

public class RecoveryIndexImage
{
    public string? FileName { get; set; }

    public List<Transform>? TransformList { get; set; }

    public ScanBitDepth BitDepth { get; set; }

    public bool HighQuality { get; set; }
}