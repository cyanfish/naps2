using NAPS2.ImportExport.Pdf.Pdfium;

namespace NAPS2.ImportExport.Pdf;

public static class CcittReader
{
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

    public static IMemoryImage LoadRawCcitt(ImageContext imageContext, byte[] buffer, PdfImageMetadata metadata)
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
        return imageContext.Load(stream);
    }

    private static void Write(MemoryStream stream, byte[] bytes)
    {
        stream.Write(bytes, 0, bytes.Length);
    }

    private static void Write(MemoryStream stream, int value)
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