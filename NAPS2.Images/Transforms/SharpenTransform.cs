
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace NAPS2.Images.Transforms;

public record SharpenTransform : Transform
{
    public SharpenTransform()
    {
    }

    public SharpenTransform(int sharpness)
    {
        Sharpness = sharpness;
    }

    public int Sharpness { get; private set; }

    public override bool IsNull => Sharpness == 0;
}