using System.Collections.Immutable;
using NAPS2.Util;

namespace NAPS2.Images;

// TODO: Make sure transform equality works
public record TransformState(ImmutableList<Transform> Transforms)
{
    public static readonly TransformState Empty = new(ImmutableList<Transform>.Empty);

    public bool IsEmpty => Transforms.IsEmpty;

    public TransformState AddOrSimplify(Transform transform)
    {
        if (transform.IsNull)
        {
            return this;
        }
        return new TransformState(Transform.AddOrSimplify(Transforms, transform));
    }

    public virtual bool Equals(TransformState? other)
    {
        if (other == null)
        {
            return false;
        }

        return ObjectHelpers.ListEquals(Transforms, other.Transforms);
    }

    public override int GetHashCode()
    {
        return ObjectHelpers.ListHashCode(Transforms);
    }
}
