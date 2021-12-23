using System.Collections.Immutable;

namespace NAPS2.Images;

public record Memento(ImmutableList<RenderableImage> Images) : IDisposable
{
    public static readonly Memento Empty = new Memento(ImmutableList.Create<RenderableImage>());

    public void Dispose()
    {
        foreach (var image in Images)
        {
            image.Dispose();
        }
    }
}