#if !MAC
using NAPS2.Images.Bitwise;
using NAPS2.Remoting.Worker;
using NTwain.Data;

namespace NAPS2.Scan.Internal.Twain;

/// <summary>
/// For Twain MemXfer, this class reads the raw buffer data and copies it into an image object. 
/// </summary>
public static class TwainMemoryBufferReader
{
    public static void CopyBufferToImage(TwainMemoryBuffer memoryBuffer, TwainImageData imageData,
        IMemoryImage outputImage)
    {
        var subPixelType = ((PixelType) imageData.PixelType, imageData.BitsPerPixel, imageData.SamplesPerPixel,
                imageData.BitsPerSample) switch
            {
                (PixelType.RGB, 24, 3, [8, ..]) => SubPixelType.Rgb,
                (PixelType.Gray, 8, 1, [8, ..]) => SubPixelType.Gray,
                (PixelType.BlackWhite, 1, 1, [1, ..]) => SubPixelType.Bit,
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