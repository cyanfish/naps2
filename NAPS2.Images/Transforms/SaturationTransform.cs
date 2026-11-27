using NAPS2.Serialization;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace NAPS2.Images.Transforms;

[GeneratePropertyAccessors]
public record SaturationTransform : Transform
{
    public SaturationTransform()
    {
    }

    public SaturationTransform(int saturation)
    {
        Saturation = saturation;
    }

    public int Saturation { get; private set; }

    public override bool IsNull => Saturation == 0;
}