
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace NAPS2.Images.Transforms;

public class ThumbnailTransform : Transform
{
    public const int DEFAULT_SIZE = 256;

    public ThumbnailTransform()
    {
        Size = DEFAULT_SIZE;
    }

    public ThumbnailTransform(int size)
    {
        Size = size;
    }

    public int Size { get; private set; }
}