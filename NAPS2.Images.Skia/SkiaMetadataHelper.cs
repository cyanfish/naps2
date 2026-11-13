using System.Buffers.Binary;
using NAPS2.ImportExport;
using SkiaSharp;

namespace NAPS2.Images.Skia;

/// <summary>
/// Skia doesn't read or write image resolution metadata, so this reads and writes it directly in the encoded files.
/// </summary>
internal static class SkiaMetadataHelper
{
    private const double INCHES_PER_METER = 1 / 0.0254;

    // The largest value that can be stored in a JFIF header. This also excludes infinity and NaN, which can result
    // from malformed metadata.
    private const double MAX_RESOLUTION = ushort.MaxValue;

    private const int PNG_SIGNATURE_LENGTH = 8;
    // Chunk length (4) + type (4) + IHDR data (13) + CRC (4)
    private const int PNG_IHDR_CHUNK_LENGTH = 25;
    private const int PNG_PHYS_DATA_LENGTH = 9;

    private static readonly uint[] Crc32Table = CreateCrc32Table();

    public static (float xDpi, float yDpi) ReadResolution(SKData data, ImageFileFormat format)
    {
        try
        {
            var span = data.AsSpan();
            if (format == ImageFileFormat.Unknown && span.Length >= 8)
            {
                format = ImageContext.GetFileFormatFromFirstBytes(span.Slice(0, 8).ToArray());
            }
            var (xDpi, yDpi) = format switch
            {
                ImageFileFormat.Jpeg => ReadJpegResolution(data),
                ImageFileFormat.Png => ReadPngResolution(span),
                ImageFileFormat.Bmp => ReadBmpResolution(span),
                _ => (0, 0)
            };
            return IsValidResolution(xDpi) && IsValidResolution(yDpi) ? ((float) xDpi, (float) yDpi) : (0, 0);
        }
        catch (Exception)
        {
            // Malformed metadata shouldn't prevent us from loading an image that Skia was able to decode
            return (0, 0);
        }
    }

    private static bool IsValidResolution(double dpi) => dpi > 0 && dpi <= MAX_RESOLUTION;

    private static (double, double) ReadJpegResolution(SKData data)
    {
        // This reads directly from the native memory without copying
        using var stream = data.AsStream();
        var header = JpegFormatHelper.ReadHeader(stream);
        return header == null ? (0, 0) : (header.HorizontalDpi, header.VerticalDpi);
    }

    private static (double, double) ReadPngResolution(ReadOnlySpan<byte> data)
    {
        int offset = FindPngChunk(data, "pHYs"u8);
        if (offset == -1 || BinaryPrimitives.ReadUInt32BigEndian(data.Slice(offset)) != PNG_PHYS_DATA_LENGTH)
        {
            return (0, 0);
        }
        var xPixelsPerUnit = BinaryPrimitives.ReadUInt32BigEndian(data.Slice(offset + 8));
        var yPixelsPerUnit = BinaryPrimitives.ReadUInt32BigEndian(data.Slice(offset + 12));
        var unit = data[offset + 16];
        // Unit 1 is meters; unit 0 is unspecified (i.e. only the aspect ratio is meaningful)
        return unit == 1 ? (PixelsPerMeterToDpi(xPixelsPerUnit), PixelsPerMeterToDpi(yPixelsPerUnit)) : (0, 0);
    }

    /// <summary>
    /// Gets the offset of the first chunk with the given type that's fully contained in the data and comes before
    /// the image data, or -1 if there isn't one.
    /// </summary>
    private static int FindPngChunk(ReadOnlySpan<byte> data, ReadOnlySpan<byte> chunkType)
    {
        // PNG format doc: https://www.w3.org/TR/png/
        long offset = PNG_SIGNATURE_LENGTH;
        while (offset + 12 <= data.Length)
        {
            long length = BinaryPrimitives.ReadUInt32BigEndian(data.Slice((int) offset));
            var type = data.Slice((int) offset + 4, 4);
            if (offset + 12 + length > data.Length || type.SequenceEqual("IDAT"u8))
            {
                break;
            }
            if (type.SequenceEqual(chunkType))
            {
                return (int) offset;
            }
            offset += 12 + length;
        }
        return -1;
    }

    private static (double, double) ReadBmpResolution(ReadOnlySpan<byte> data)
    {
        // The file header is 14 bytes, followed by the info header. The resolution fields only exist in
        // BITMAPINFOHEADER (40 bytes) and its later versions.
        if (data.Length < 54 || BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(14)) < 40)
        {
            return (0, 0);
        }
        var xPixelsPerMeter = BinaryPrimitives.ReadInt32LittleEndian(data.Slice(38));
        var yPixelsPerMeter = BinaryPrimitives.ReadInt32LittleEndian(data.Slice(42));
        return (PixelsPerMeterToDpi(xPixelsPerMeter), PixelsPerMeterToDpi(yPixelsPerMeter));
    }

    /// <summary>
    /// Converts to DPI, snapping to a whole number if that's consistent with the stored value. PNG and BMP store
    /// resolution as an integer number of pixels per meter, so e.g. 300 DPI is stored as 11811, which would otherwise
    /// be read back as 299.9994.
    /// </summary>
    private static double PixelsPerMeterToDpi(long pixelsPerMeter)
    {
        var dpi = pixelsPerMeter / INCHES_PER_METER;
        var wholeDpi = Math.Round(dpi);
        return Math.Round(wholeDpi * INCHES_PER_METER) == pixelsPerMeter ? wholeDpi : dpi;
    }

    /// <summary>
    /// Returns the encoded image with the resolution set in its metadata.
    /// </summary>
    public static byte[] WriteResolution(byte[] data, ImageFileFormat format, float xDpi, float yDpi)
    {
        // Each format stores the resolution as an integer, so it needs to be at least 1 after rounding
        if (!IsValidResolution(Math.Round(xDpi)) || !IsValidResolution(Math.Round(yDpi)))
        {
            return data;
        }
        return format switch
        {
            ImageFileFormat.Jpeg => WriteJpegResolution(data, xDpi, yDpi),
            ImageFileFormat.Png => WritePngResolution(data, xDpi, yDpi),
            _ => data
        };
    }

    private static byte[] WriteJpegResolution(byte[] data, float xDpi, float yDpi)
    {
        var xDensity = (ushort) Math.Round(xDpi);
        var yDensity = (ushort) Math.Round(yDpi);
        // Skia writes a JFIF header directly after the start-of-image marker, so we only need to update the density.
        // Layout: SOI (2), APP0 marker (2), length (2), "JFIF\0" (5), version (2), units (1), x (2), y (2)
        if (data.Length < 18 || !data.AsSpan(0, 4).SequenceEqual(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }) ||
            !data.AsSpan(6, 5).SequenceEqual("JFIF\0"u8))
        {
            return data;
        }
        data[13] = 1; // Pixels per inch
        BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(14), xDensity);
        BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(16), yDensity);
        return data;
    }

    private static byte[] WritePngResolution(byte[] data, float xDpi, float yDpi)
    {
        // The IHDR chunk is always first, and we insert the pHYs chunk directly after it
        int insertOffset = PNG_SIGNATURE_LENGTH + PNG_IHDR_CHUNK_LENGTH;
        if (data.Length < insertOffset || !data.AsSpan(12, 4).SequenceEqual("IHDR"u8))
        {
            return data;
        }
        if (FindPngChunk(data, "pHYs"u8) != -1)
        {
            // Skia doesn't currently write this chunk, but if that changes, a second one would make the file invalid
            return data;
        }
        var chunk = new byte[12 + PNG_PHYS_DATA_LENGTH];
        BinaryPrimitives.WriteUInt32BigEndian(chunk, PNG_PHYS_DATA_LENGTH);
        "pHYs"u8.CopyTo(chunk.AsSpan(4));
        BinaryPrimitives.WriteUInt32BigEndian(chunk.AsSpan(8), (uint) Math.Round(xDpi * INCHES_PER_METER));
        BinaryPrimitives.WriteUInt32BigEndian(chunk.AsSpan(12), (uint) Math.Round(yDpi * INCHES_PER_METER));
        chunk[16] = 1; // Meters
        // The CRC covers the chunk type and data but not the length
        BinaryPrimitives.WriteUInt32BigEndian(chunk.AsSpan(17), Crc32(chunk.AsSpan(4, 4 + PNG_PHYS_DATA_LENGTH)));

        var result = new byte[data.Length + chunk.Length];
        data.AsSpan(0, insertOffset).CopyTo(result);
        chunk.CopyTo(result.AsSpan(insertOffset));
        data.AsSpan(insertOffset).CopyTo(result.AsSpan(insertOffset + chunk.Length));
        return result;
    }

    private static uint Crc32(ReadOnlySpan<byte> data)
    {
        uint crc = 0xFFFFFFFF;
        foreach (var b in data)
        {
            crc = Crc32Table[(crc ^ b) & 0xFF] ^ (crc >> 8);
        }
        return crc ^ 0xFFFFFFFF;
    }

    private static uint[] CreateCrc32Table()
    {
        var table = new uint[256];
        for (uint i = 0; i < 256; i++)
        {
            uint value = i;
            for (int bit = 0; bit < 8; bit++)
            {
                value = (value & 1) != 0 ? 0xEDB88320 ^ (value >> 1) : value >> 1;
            }
            table[i] = value;
        }
        return table;
    }
}
