using NAPS2.ImportExport.Pdf.Pdfium;

namespace NAPS2.ImportExport.Pdf;

public class PdfiumBitmapFactory
{
    private readonly ImageContext _imageContext;

    public PdfiumBitmapFactory(ImageContext imageContext)
    {
        _imageContext = imageContext;
    }

    public unsafe IMemoryImage CopyPdfBitmapToNewImage(PdfBitmap pdfBitmap, PdfImageMetadata imageMetadata)
    {
        var dstImage = _imageContext.Create(pdfBitmap.Width, pdfBitmap.Height, pdfBitmap.Format);
        dstImage.SetResolution(imageMetadata.HorizontalDpi, imageMetadata.VerticalDpi);
        using var imageLock = dstImage.Lock(LockMode.ReadWrite, out var dstBuffer, out var dstStride);
        var srcBuffer = pdfBitmap.Buffer;
        var srcStride = pdfBitmap.Stride;
        for (int y = 0; y < dstImage.Height; y++)
        {
            IntPtr srcRow = srcBuffer + srcStride * y;
            IntPtr dstRow = dstBuffer + dstStride * y;
            Buffer.MemoryCopy(srcRow.ToPointer(), dstRow.ToPointer(), dstStride, Math.Min(srcStride, dstStride));
        }

        return dstImage;
    }

    public IMemoryImage RenderPageToNewImage(PdfPage page, float defaultDpi)
    {
        var widthInInches = page.Width / 72;
        var heightInInches = page.Height / 72;

        // Cap the resolution to 10k pixels in each dimension
        var dpi = defaultDpi;
        dpi = Math.Min(dpi, 10000 / heightInInches);
        dpi = Math.Min(dpi, 10000 / widthInInches);

        int widthInPx = (int) Math.Round(widthInInches * dpi);
        int heightInPx = (int) Math.Round(heightInInches * dpi);

        var bitmap = _imageContext.Create(widthInPx, heightInPx, ImagePixelFormat.RGB24);
        bitmap.SetResolution(dpi, dpi);
        using var bitmapData = bitmap.Lock(LockMode.ReadWrite, out var scan0, out var stride);
        using var pdfiumBitmap = PdfBitmap.CreateFromPointerBgr(widthInPx, heightInPx, scan0, stride);
        pdfiumBitmap.FillRect(0, 0, widthInPx, heightInPx, PdfBitmap.WHITE);
        pdfiumBitmap.RenderPage(page, 0, 0, widthInPx, heightInPx);
        return bitmap;
    }

    public unsafe IMemoryImage LoadRawRgb(byte[] buffer, PdfImageMetadata metadata)
    {
        var image = _imageContext.Create(metadata.Width, metadata.Height, ImagePixelFormat.RGB24);
        image.OriginalFileFormat = ImageFileFormat.Png;

        using var data = image.Lock(LockMode.WriteOnly, out var scan0, out var stride);
        int height = image.Height;
        int width = image.Width;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var pixelData = (byte*) (scan0 + y * stride + x * 3);
                int bufferIndex = (y * width + x) * 3;
                *pixelData = buffer[bufferIndex + 2];
                *(pixelData + 1) = buffer[bufferIndex + 1];
                *(pixelData + 2) = buffer[bufferIndex];
            }
        }
        return image;
    }

    public unsafe IMemoryImage LoadRawBlackAndWhite(byte[] buffer, PdfImageMetadata metadata)
    {
        var image = _imageContext.Create(metadata.Width, metadata.Height, ImagePixelFormat.RGB24);
        image.OriginalFileFormat = ImageFileFormat.Png;

        using var data = image.Lock(LockMode.WriteOnly, out var scan0, out var stride);
        int height = image.Height;
        int width = image.Width;
        int bytesPerRow = (width - 1) / 8 + 1;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < bytesPerRow; x++)
            {
                var pixelData = (byte*) (scan0 + y * stride + x);
                *pixelData = buffer[y * bytesPerRow + x];
            }
        }
        return image;
    }

    private static readonly byte[] TiffBeforeDataLen = { 0x49, 0x49, 0x2A, 0x00 };
    private static readonly byte[] TiffBeforeData = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
    private static readonly byte[] TiffBeforeWidth = { 0x07, 0x00, 0x00, 0x01, 0x03, 0x00, 0x01, 0x00, 0x00, 0x00 };
    private static readonly byte[] TiffBeforeHeight = { 0x01, 0x01, 0x03, 0x00, 0x01, 0x00, 0x00, 0x00 };
    private static readonly byte[] TiffBeforeBits = { 0x02, 0x01, 0x03, 0x00, 0x01, 0x00, 0x00, 0x00 };
    private static readonly byte[] TiffBeforeRealLen =
    {
        0x03, 0x01, 0x03, 0x00, 0x01, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x11, 0x01, 0x04, 0x00, 0x01, 0x00,
        0x00, 0x00, 0x10, 0x00, 0x00, 0x00, 0x15, 0x01, 0x03, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
        0x17, 0x01, 0x04, 0x00, 0x01, 0x00, 0x00, 0x00
    };
    private static readonly byte[] TiffTrailer = { 0x00, 0x00, 0x00, 0x00 };

    // Sample full tiff          LEN-------------------                                                  DATA------------------                                                              WIDTH-----------------                                                  HEIGHT----------------                                                  BITS PER COMP---------                                                                                                                                                                                                                                                                          REALLEN---------------
    // { 0x49, 0x49, 0x2A, 0x00, 0x14, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x99, 0x99, 0x99, 0x99, 0x07, 0x00, 0x00, 0x01, 0x03, 0x00, 0x01, 0x00, 0x00, 0x00, 0x77, 0x77, 0x00, 0x00, 0x01, 0x01, 0x03, 0x00, 0x01, 0x00, 0x00, 0x00, 0x88, 0x88, 0x00, 0x00, 0x02, 0x01, 0x03, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x03, 0x01, 0x03, 0x00, 0x01, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x11, 0x01, 0x04, 0x00, 0x01, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 0x00, 0x15, 0x01, 0x03, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x17, 0x01, 0x04, 0x00, 0x01, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

    public IMemoryImage LoadRawCcitt(byte[] buffer, PdfImageMetadata metadata)
    {
        // We don't have easy access to a standalone CCITT G4 decoder, so we'll make use of the .NET TIFF decoder
        // by constructing a valid TIFF file "manually" and directly injecting the bytestream
        var stream = new MemoryStream();
        Write(stream, TiffBeforeDataLen);
        // The bytestream is 2-padded, so we may need to append an extra zero byte
        if (buffer.Length % 2 == 1)
        {
            Write(stream, buffer.Length + 0x11);
        }
        else
        {
            Write(stream, buffer.Length + 0x10);
        }
    
        Write(stream, TiffBeforeData);
        Write(stream, buffer);
        if (buffer.Length % 2 == 1)
        {
            Write(stream, new byte[] { 0x00 });
        }
    
        Write(stream, TiffBeforeWidth);
        Write(stream, metadata.Width);
        Write(stream, TiffBeforeHeight);
        Write(stream, metadata.Height);
        Write(stream, TiffBeforeBits);
        Write(stream, 1); // bits per component
        Write(stream, TiffBeforeRealLen);
        Write(stream, buffer.Length);
        Write(stream, TiffTrailer);
        stream.Seek(0, SeekOrigin.Begin);
    
        // TODO: If we need a TIFF hint for loading, it should go here.
        return _imageContext.Load(stream);
    }

    private void Write(MemoryStream stream, byte[] bytes)
    {
        stream.Write(bytes, 0, bytes.Length);
    }

    private void Write(MemoryStream stream, int value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }

        Debug.Assert(bytes.Length == 4);
        stream.Write(bytes, 0, bytes.Length);
    }
}