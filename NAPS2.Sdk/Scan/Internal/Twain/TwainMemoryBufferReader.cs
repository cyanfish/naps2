#if !MAC
using NAPS2.Images.Bitwise;
using NAPS2.Remoting.Worker;

namespace NAPS2.Scan.Internal.Twain;

/// <summary>
/// For Twain MemXfer, this class reads the raw buffer data and copies it into an image object. 
/// </summary>
public static class TwainMemoryBufferReader
{
    private const int BLACK_WHITE = 0;
    private const int GRAY = 1;
    private const int RGB = 2;

    public static void CopyBufferToImage(TwainMemoryBuffer memoryBuffer, TwainImageData imageData,
        IMemoryImage outputImage)
    {
        var subPixelType = (imageData.PixelType, imageData.BitsPerPixel, imageData.SamplesPerPixel,
                imageData.BitsPerSample) switch
            {
                // Technically for RGB we should check for [8, 8, 8, ...] but some scanners only set the first value
                (RGB, 24, 3, [8, ..]) => SubPixelType.Rgb,
                (GRAY, 8, 1, [8, ..]) => SubPixelType.Gray,
                (BLACK_WHITE, 1, 1, [1, ..]) => SubPixelType.Bit,
                _ => throw new ArgumentException(
                    $"Unsupported pixel type: {imageData.BitsPerPixel} {imageData.PixelType} {imageData.SamplesPerPixel} {string.Join(",", imageData.BitsPerSample)}")
            };
        var pixelInfo = new PixelInfo(memoryBuffer.Columns, memoryBuffer.Rows, subPixelType, memoryBuffer.BytesPerRow);
        new CopyBitwiseImageOp
        {
            DestXOffset = memoryBuffer.XOffset,
            DestYOffset = memoryBuffer.YOffset
        }.Perform(memoryBuffer.Buffer.ToByteArray(), pixelInfo, outputImage);
    }
}
#endif