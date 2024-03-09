
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace NAPS2.Images.Transforms;

public record ResizeTransform : Transform
{
    public ResizeTransform()
    {
    }

    public ResizeTransform(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public int Width { get; private set; }

    public int Height { get; private set; }
}