using NAPS2.Serialization;

namespace NAPS2.Config;

[GeneratePropertyAccessors]
public class ManualDuplexSettings
{
    public bool AlwaysShowPreview { get; set; }

    public bool ReverseBackSides { get; set; }
}