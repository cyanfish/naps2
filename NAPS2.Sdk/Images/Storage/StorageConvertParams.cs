using NAPS2.Scan;

namespace NAPS2.Images.Storage;

public class StorageConvertParams
{
    public bool Temporary { get; set; }

    public bool Lossless { get; set; }

    public int LossyQuality { get; set; }

    public BitDepth BitDepth { get; set; }
}