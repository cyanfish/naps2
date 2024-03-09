
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace NAPS2.Images.Transforms;

public record ThumbnailTransform : Transform
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

    public (int left, int top, int width, int height) GetDrawRect(int originalWidth, int originalHeight)
    {
        // The location and dimensions of the old bitmap, scaled and positioned within the thumbnail bitmap
        int left, top, width, height;

        if (originalWidth > originalHeight)
        {
            // Fill the new bitmap's width
            width = Size;
            left = 0;
            // Scale the drawing height to match the original bitmap's aspect ratio
            height = (int)(originalHeight * (Size / (double)originalWidth));
            // Center the drawing vertically
            top = (Size - height) / 2;
        }
        else
        {
            // Fill the new bitmap's height
            height = Size;
            top = 0;
            // Scale the drawing width to match the original bitmap's aspect ratio
            width = (int)(originalWidth * (Size / (double)originalHeight));
            // Center the drawing horizontally
            left = (Size - width) / 2;
        }
        return (left, top, width, height);
    }
}