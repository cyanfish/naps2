using NAPS2.Remoting.Worker;

namespace NAPS2.Scan.Internal.Twain;

public class TwainMemoryBufferReader
{
    private readonly ScanningContext _scanningContext;

    public TwainMemoryBufferReader(ScanningContext scanningContext)
    {
        _scanningContext = scanningContext;
    }

    public IMemoryImage ReadBuffer(MemoryStream buffer, TwainImageData imageData)
    {
        //Log.Error($"memoryData.Length: {memoryData.Length}, ImageWidth: {imageInfo.ImageWidth}, ImageLength: {imageInfo.ImageLength}, BitsPerPixel: {imageInfo.BitsPerPixel}, SamplesPerPixel: {imageInfo.SamplesPerPixel}, BitsPerSample[0]: {imageInfo.BitsPerSample[0]}, Compression: {imageInfo.Compression}, PixelType: {imageInfo.PixelType}, XRes: {imageInfo.XResolution}, YRes: {imageInfo.YResolution}");
        throw new Exception("blah");
        // int bytesPerPixel = memoryData.Length / (imageInfo.ImageWidth * imageInfo.ImageLength);
        // var pixelFormat = bytesPerPixel == 0 ? ImagePixelFormat.BW1 : ImagePixelFormat.RGB24;
        // int imageWidth = imageInfo.ImageWidth;
        // int imageHeight = imageInfo.ImageLength;
        // var bitmap = _scanningContext.ImageContext.Create(imageWidth, imageHeight, pixelFormat);
        // var data = bitmap.Lock(LockMode.WriteOnly, out var scan0, out var stride);
        // try
        // {
        //     byte[] source = memoryData;
        //     if (bytesPerPixel == 1)
        //     {
        //         // No 8-bit greyscale format, so we have to transform into 24-bit
        //         int rowWidth = stride;
        //         int originalRowWidth = source.Length / imageHeight;
        //         byte[] source2 = new byte[rowWidth * imageHeight];
        //         for (int row = 0; row < imageHeight; row++)
        //         {
        //             for (int col = 0; col < imageWidth; col++)
        //             {
        //                 source2[row * rowWidth + col * 3] = source[row * originalRowWidth + col];
        //                 source2[row * rowWidth + col * 3 + 1] = source[row * originalRowWidth + col];
        //                 source2[row * rowWidth + col * 3 + 2] = source[row * originalRowWidth + col];
        //             }
        //         }
        //         source = source2;
        //     }
        //     else if (bytesPerPixel == 3)
        //     {
        //         // Colors are provided as BGR, they need to be swapped to RGB
        //         int rowWidth = stride;
        //         for (int row = 0; row < imageHeight; row++)
        //         {
        //             for (int col = 0; col < imageWidth; col++)
        //             {
        //                 (source[row * rowWidth + col * 3], source[row * rowWidth + col * 3 + 2]) =
        //                     (source[row * rowWidth + col * 3 + 2], source[row * rowWidth + col * 3]);
        //             }
        //         }
        //     }
        //     Marshal.Copy(source, 0, scan0, source.Length);
        // }
        // finally
        // {
        //     bitmap.Unlock(data);
        // }
        // return bitmap;
    }
}