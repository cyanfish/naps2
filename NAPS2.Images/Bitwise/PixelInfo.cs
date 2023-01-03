namespace NAPS2.Images.Bitwise;

public class PixelInfo
{
    public PixelInfo(int width, int height, SubPixelType subPixelType, int stride = -1, int strideAlign = -1)
    {
        var minStride = (width * subPixelType.BitsPerPixel + 7) / 8;
        if (stride == -1)
        {
            stride = minStride;
        }
        else if (stride < minStride)
        {
            throw new ArgumentException("Invalid stride");
        }
        if (strideAlign > 0)
        {
            stride = (stride + (strideAlign - 1)) / strideAlign * strideAlign;
        }
        Width = width;
        Height = height;
        SubPixelType = subPixelType;
        Stride = stride;
        Length = stride * height;
    }

    public int Width { get; }
    public int Height { get; }
    public SubPixelType SubPixelType { get; }
    public int Stride { get; }
    public long Length { get; }
    public bool InvertY { get; init; }
}