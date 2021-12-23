using System.Collections.Immutable;

namespace NAPS2.Images;

// TODO: Make sure transform equality works
public record TransformState(ImmutableList<Transform> Transforms)
{
    public static readonly TransformState Empty = new(ImmutableList<Transform>.Empty);

    public bool IsEmpty => Transforms.IsEmpty;
}
