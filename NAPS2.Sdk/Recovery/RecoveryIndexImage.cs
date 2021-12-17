using NAPS2.Scan;
using System.Collections.Generic;
using NAPS2.Images.Transforms;

namespace NAPS2.Recovery;

public class RecoveryIndexImage
{
    public string? FileName { get; set; }

    public List<Transform>? TransformList { get; set; }

    public ScanBitDepth BitDepth { get; set; }

    public bool HighQuality { get; set; }
}