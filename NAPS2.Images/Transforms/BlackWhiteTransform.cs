
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace NAPS2.Images.Transforms;

public record BlackWhiteTransform : Transform
{
    public BlackWhiteTransform()
    {
    }

    public BlackWhiteTransform(int threshold)
    {
        Threshold = threshold;
    }
        
    public int Threshold { get; private set; }
}