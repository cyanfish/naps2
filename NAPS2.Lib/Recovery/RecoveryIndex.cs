using NAPS2.Serialization;

namespace NAPS2.Recovery;

[GeneratePropertyAccessors]
public class RecoveryIndex
{
    public const int CURRENT_VERSION = 1;

    public static RecoveryIndex Create() => new RecoveryIndex { Version = CURRENT_VERSION };

    public RecoveryIndex()
    {
        Images = [];
    }

    public int Version { get; set; }

    public List<RecoveryIndexImage> Images { get; set; }
}