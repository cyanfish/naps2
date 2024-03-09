
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace NAPS2.Images.Transforms;

public record HueTransform : Transform
{
    public HueTransform()
    {
    }

    public HueTransform(int hueShift)
    {
        HueShift = hueShift;
    }

    public int HueShift { get; private set; }

    public override bool CanSimplify(Transform other) => other is HueTransform;

    public override Transform Simplify(Transform other)
    {
        var other2 = (HueTransform)other;
        return new HueTransform((HueShift + other2.HueShift + 3000) % 2000 - 1000);
    }

    public override bool IsNull => HueShift == 0;
}