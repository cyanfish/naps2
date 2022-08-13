namespace NAPS2.Images.Storage;

public enum ImagePixelFormat
{
    Unsupported,
    BW1,
    Gray8,
    RGB24, // This is actually BGR in the binary representation
    ARGB32
}