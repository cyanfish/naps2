using NAPS2.Serialization;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace NAPS2.Images.Transforms;

[GeneratePropertyAccessors]
public record TrueContrastTransform : Transform
{
    public TrueContrastTransform()
    {
    }

    public TrueContrastTransform(int contrast)
    {
        Contrast = contrast;
    }

    public int Contrast { get; private set; }

    public override bool IsNull => Contrast == 0;
}