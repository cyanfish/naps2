// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace NAPS2.Images.Transforms;

/// <summary>
/// Sets the image's resolution (DPI) without changing its pixels.
/// </summary>
public record ResolutionTransform : Transform
{
    public ResolutionTransform()
    {
    }

    public ResolutionTransform(float dpi)
    {
        Dpi = dpi;
    }

    public float Dpi { get; private set; }

    public override bool IsNull => Dpi <= 0;

    public override bool CanSimplify(Transform other) => other is ResolutionTransform;

    public override Transform Simplify(Transform other) => this;
}
