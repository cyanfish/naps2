using System.Collections.Immutable;

namespace NAPS2.Images;

/// <summary>
/// A memento represents a snapshot of the current set of images in the application to allow undo/redo.
/// https://en.wikipedia.org/wiki/Memento_pattern
///
/// Mementos are equal if their contents are equal. This allows no-ops to be identified so that you don't hit Ctrl+Z and
/// have nothing happen.
/// </summary>
public record Memento(ImmutableList<RenderableImage> Images) : IDisposable
{
    public static readonly Memento Empty = new Memento(ImmutableList.Create<RenderableImage>());

    // TODO: Do we need to implement equality better at the RenderableImage level?
    public virtual bool Equals(Memento? other)
    {
        if (other == null)
        {
            return false;
        }

        if (other.Images.Count != Images.Count)
        {
            return false;
        }

        for (int i = 0; i < Images.Count; i++)
        {
            if (other.Images[i] != Images[i])
            {
                return false;
            }
        }

        return true;
    }

    public override int GetHashCode()
    {
        return Images.Aggregate(0, (value, image) => (value * 397) ^ image.GetHashCode());
    }

    public void Dispose()
    {
        // TODO: We need to make ownership explicit here
        foreach (var image in Images)
        {
            image.Dispose();
        }
    }
}