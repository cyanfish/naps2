using NAPS2.Serialization;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace NAPS2.Images.Transforms;

[GeneratePropertyAccessors]
public record BrightnessTransform : Transform
{
    public BrightnessTransform()
    {
    }

    public BrightnessTransform(int brightness)
    {
        Brightness = brightness;
    }

    public int Brightness { get; private set; }

    public override bool IsNull => Brightness == 0;
}