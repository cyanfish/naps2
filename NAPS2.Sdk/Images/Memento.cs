using System.Collections.Immutable;

namespace NAPS2.Images;

/// <summary>
/// A memento represents a snapshot of the current set of images in the application to allow undo/redo.
/// https://en.wikipedia.org/wiki/Memento_pattern
///
/// Mementos are equal if their contents are equal. This allows no-ops to be identified so that you don't hit Ctrl+Z and
/// have nothing happen.
///
/// When you create a memento with a list of ProcessedImage instances, the memento takes ownership of those instances.
/// When the memento is disposed they are disposed.
/// </summary>
public record Memento(ImmutableList<ProcessedImage> Images) : IDisposable
{
    public static readonly Memento Empty = new(ImmutableList.Create<ProcessedImage>());

    // TODO: Do we need to implement equality better at the RenderableImage level?
    public virtual bool Equals(Memento? other)
    {
        if (other == null)
        {
            return false;
        }

        return ObjectHelpers.ListEquals(Images, other.Images);
    }

    public override int GetHashCode()
    {
        return ObjectHelpers.ListHashCode(Images);
    }

    public void Dispose()
    {
        Images.DisposeAll();
    }
}