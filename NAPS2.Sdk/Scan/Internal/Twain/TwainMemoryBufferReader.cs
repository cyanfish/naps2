using NAPS2.Remoting.Worker;
using NTwain.Data;

namespace NAPS2.Scan.Internal.Twain;

/// <summary>
/// For Twain MemXfer, this class reads the raw buffer data and copies it into an image object. 
/// </summary>
public static class TwainMemoryBufferReader
{
    public static unsafe void CopyBufferToImage(TwainMemoryBuffer memoryBuffer, TwainImageData imageData,
        IMemoryImage outputImage)
    {
        var data = outputImage.Lock(LockMode.WriteOnly, out var scan0, out var dstBytesPerRow);
        try
        {
            byte[] source = memoryBuffer.Buffer.ToByteArray();
            if (memoryBuffer.BytesPerRow < memoryBuffer.Columns * (imageData.BitsPerPixel / 8.0) ||
                source.Length < memoryBuffer.BytesPerRow * memoryBuffer.Rows ||
                memoryBuffer.XOffset < 0 ||
                memoryBuffer.YOffset < 0 ||
                memoryBuffer.XOffset + memoryBuffer.Columns > imageData.Width ||
                memoryBuffer.YOffset + memoryBuffer.Rows > imageData.Height)
            {
                throw new ArgumentException(
                    $"Invalid buffer parameters: {memoryBuffer.BytesPerRow} {memoryBuffer.Columns} {memoryBuffer.Rows} {source.Length} {memoryBuffer.XOffset} {memoryBuffer.YOffset}");
            }

            var srcBytesPerRow = memoryBuffer.BytesPerRow;
            byte* dstPtr = (byte*) scan0.ToPointer();

            if (imageData.BitsPerPixel == 1)
            {
                // Black & white
                if (outputImage.PixelFormat != ImagePixelFormat.BW1) throw new ArgumentException();
                if (imageData.PixelType != (int) PixelType.BlackWhite || imageData.SamplesPerPixel != 1 ||
                    imageData.BitsPerSample.Count < 1 || imageData.BitsPerSample[0] != 1)
                {
                    ThrowForUnsupportedPixelType(imageData);
                }
                if (memoryBuffer.XOffset % 8 != 0)
                {
                    throw new ArgumentException("Unaligned offset for 1bpp image");
                }
                fixed (byte* srcPtr = &source[0])
                {
                    for (int dy = 0; dy < memoryBuffer.Rows; dy++)
                    {
                        for (int dx = 0; dx < (memoryBuffer.Columns + 7) / 8; dx++)
                        {
                            int x = memoryBuffer.XOffset / 8 + dx;
                            int y = memoryBuffer.YOffset + dy;
                            // Copy 8 bits at a time
                            *(dstPtr + y * dstBytesPerRow + x) = *(srcPtr + dy * srcBytesPerRow + dx);
                        }
                    }
                }
            }
            else if (imageData.BitsPerPixel == 8)
            {
                // Grayscale
                if (outputImage.PixelFormat != ImagePixelFormat.RGB24) throw new ArgumentException();
                if (imageData.PixelType != (int) PixelType.Gray || imageData.SamplesPerPixel != 1 ||
                    imageData.BitsPerSample.Count < 1 || imageData.BitsPerSample[0] != 8)
                {
                    ThrowForUnsupportedPixelType(imageData);
                }
                fixed (byte* srcPtr = &source[0])
                {
                    for (int dy = 0; dy < memoryBuffer.Rows; dy++)
                    {
                        for (int dx = 0; dx < memoryBuffer.Columns; dx++)
                        {
                            int x = memoryBuffer.XOffset + dx;
                            int y = memoryBuffer.YOffset + dy;
                            // No 8-bit greyscale format, so we have to transform into 24-bit RGB
                            // R
                            *(dstPtr + y * dstBytesPerRow + x * 3) = *(srcPtr + dy * srcBytesPerRow + dx);
                            // G
                            *(dstPtr + y * dstBytesPerRow + x * 3 + 1) = *(srcPtr + dy * srcBytesPerRow + dx);
                            // B
                            *(dstPtr + y * dstBytesPerRow + x * 3 + 2) = *(srcPtr + dy * srcBytesPerRow + dx);
                        }
                    }
                }
            }
            else if (imageData.BitsPerPixel == 24)
            {
                // Color
                if (outputImage.PixelFormat != ImagePixelFormat.RGB24) throw new ArgumentException();
                if (imageData.PixelType != (int) PixelType.RGB || imageData.SamplesPerPixel != 3 ||
                    imageData.BitsPerSample.Count < 3 || imageData.BitsPerSample[0] != 8 ||
                    imageData.BitsPerSample[1] != 8 || imageData.BitsPerSample[2] != 8)
                {
                    ThrowForUnsupportedPixelType(imageData);
                }
                fixed (byte* srcPtr = &source[0])
                {
                    for (int dy = 0; dy < memoryBuffer.Rows; dy++)
                    {
                        for (int dx = 0; dx < memoryBuffer.Columns; dx++)
                        {
                            int x = memoryBuffer.XOffset + dx;
                            int y = memoryBuffer.YOffset + dy;
                            // Colors are provided as BGR, they need to be swapped to RGB
                            // R
                            *(dstPtr + y * dstBytesPerRow + x * 3) =
                                *(srcPtr + dy * srcBytesPerRow + dx * 3 + 2);
                            // G
                            *(dstPtr + y * dstBytesPerRow + x * 3 + 1) =
                                *(srcPtr + dy * srcBytesPerRow + dx * 3 + 1);
                            // B
                            *(dstPtr + y * dstBytesPerRow + x * 3 + 2) =
                                *(srcPtr + dy * srcBytesPerRow + dx * 3);
                        }
                    }
                }
            }
            else
            {
                ThrowForUnsupportedPixelType(imageData);
            }
        }
        finally
        {
            outputImage.Unlock(data);
        }
    }

    private static void ThrowForUnsupportedPixelType(TwainImageData imageData)
    {
        throw new ArgumentException(
            $"Unsupported pixel type: {imageData.BitsPerPixel} {imageData.PixelType} {imageData.SamplesPerPixel} {string.Join(",", imageData.BitsPerSample)}");
    }
}