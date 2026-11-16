using NAPS2.Serialization;

namespace NAPS2.Recovery;

[GeneratePropertyAccessors]
public class RecoveryIndexImage
{
    public string? FileName { get; set; }

    public List<Transform>? TransformList { get; set; }

    public bool HighQuality { get; set; }

    public int Quality { get; set; }

    public string? PageSize { get; set; }
}