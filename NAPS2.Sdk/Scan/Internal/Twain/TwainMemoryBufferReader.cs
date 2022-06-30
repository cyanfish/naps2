using NAPS2.Remoting.Worker;

namespace NAPS2.Scan.Internal.Twain;

public class TwainMemoryBufferReader
{
    public unsafe void ReadBuffer(TwainMemoryBuffer memoryBuffer, ImagePixelFormat pixelFormat, IMemoryImage outputImage)
    {
        var data = outputImage.Lock(LockMode.WriteOnly, out var scan0, out var dstBytesPerRow);
        try
        {
            byte[] source = memoryBuffer.Buffer.ToByteArray();
            if (pixelFormat == ImagePixelFormat.BW1)
            {
                // TODO: Handle BW1 and also grayscale
                // // No 8-bit greyscale format, so we have to transform into 24-bit
                // int rowWidth = stride;
                // int originalRowWidth = source.Length / imageHeight;
                // byte[] source2 = new byte[rowWidth * imageHeight];
                // for (int row = 0; row < imageHeight; row++)
                // {
                //     for (int col = 0; col < imageWidth; col++)
                //     {
                //         source2[row * rowWidth + col * 3] = source[row * originalRowWidth + col];
                //         source2[row * rowWidth + col * 3 + 1] = source[row * originalRowWidth + col];
                //         source2[row * rowWidth + col * 3 + 2] = source[row * originalRowWidth + col];
                //     }
                // }
                // source = source2;
            }
            else if (pixelFormat == ImagePixelFormat.RGB24)
            {
                if (memoryBuffer.BytesPerRow < memoryBuffer.Columns * 3 || source.Length < memoryBuffer.BytesPerRow * memoryBuffer.Rows)
                {
                    throw new ArgumentException();
                }
                var srcBytesPerRow = memoryBuffer.BytesPerRow;
                var bytesPerPixel = 3;
                byte* dstPtr = (byte*) scan0.ToPointer();
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
                            *(dstPtr + y * dstBytesPerRow + x * bytesPerPixel) =
                                *(srcPtr + dy * srcBytesPerRow + dx * bytesPerPixel + 2);
                            // G
                            *(dstPtr + y * dstBytesPerRow + x * bytesPerPixel + 1) =
                                *(srcPtr + dy * srcBytesPerRow + dx * bytesPerPixel + 1);
                            // B
                            *(dstPtr + y * dstBytesPerRow + x * bytesPerPixel + 2) =
                                *(srcPtr + dy * srcBytesPerRow + dx * bytesPerPixel);
                        }
                    }
                }
            }
            else
            {
                throw new ArgumentException();
            }
        }
        finally
        {
            outputImage.Unlock(data);
        }
    }

}