using System.Buffers.Binary;
using NAPS2.Util;

namespace NAPS2.ImportExport;

/// <summary>
/// Reads and writes the resolution in Exif metadata.
/// </summary>
public static class ExifHelper
{
    // https://docs.fileformat.com/image/exif/

    private const short TAG_X_RESOLUTION = 282;
    private const short TAG_Y_RESOLUTION = 283;
    private const short TAG_RESOLUTION_UNIT = 296;
    private const short TYPE_SHORT = 3;
    private const short TYPE_RATIONAL = 5;

    /// <summary>
    /// Gets whether the data has an Exif header at the given offset.
    /// </summary>
    public static bool IsExif(byte[] data, int offset = 0) =>
        offset >= 0 && data.Length >= offset + 8 && data[offset] == data[offset + 1] && data[offset] is 0x49 or 0x4D &&
        CreateReader(data, offset).ReadInt16(data, offset + 2) == 42;

    /// <summary>
    /// Reads the resolution in dots per inch from Exif metadata at the given offset, which may optionally start with
    /// the "Exif\0\0" marker. Returns null if there isn't a valid resolution.
    /// </summary>
    public static (double xDpi, double yDpi)? ReadResolution(byte[] data, int offset = 0)
    {
        if (data.Length >= offset + 6 && data.AsSpan(offset, 6).SequenceEqual("Exif\0\0"u8))
        {
            offset += 6;
        }
        if (!IsExif(data, offset))
        {
            return null;
        }
        try
        {
            var reader = CreateReader(data, offset);
            var ifdOffset = offset + reader.ReadInt32(data, offset + 4);
            var entryCount = (ushort) reader.ReadInt16(data, ifdOffset);
            double xRes = 0, yRes = 0;
            int unit = 0;
            // Entries that don't fit in the data (i.e. a wrong count) are ignored
            for (int i = 0; i < entryCount && ifdOffset + 2 + (i + 1) * 12 <= data.Length; i++)
            {
                var entry = ifdOffset + 2 + i * 12;
                var tag = reader.ReadInt16(data, entry);
                var type = reader.ReadInt16(data, entry + 2);
                if (tag is TAG_X_RESOLUTION or TAG_Y_RESOLUTION && type == TYPE_RATIONAL)
                {
                    // Rationals are 2 unsigned integers, stored at the given offset
                    var valueOffset = offset + reader.ReadInt32(data, entry + 8);
                    var value = (uint) reader.ReadInt32(data, valueOffset) /
                                (double) (uint) reader.ReadInt32(data, valueOffset + 4);
                    if (tag == TAG_X_RESOLUTION) xRes = value;
                    else yRes = value;
                }
                if (tag == TAG_RESOLUTION_UNIT && type == TYPE_SHORT)
                {
                    // Short values are stored directly in the entry
                    unit = reader.ReadInt16(data, entry + 8);
                }
            }
            // The upper bound excludes infinity (i.e. a zero denominator)
            if (xRes is > 0 and < double.PositiveInfinity && yRes is > 0 and < double.PositiveInfinity)
            {
                // 3 = centimeters; otherwise assume inches
                return unit == 3 ? (xRes * 2.54, yRes * 2.54) : (xRes, yRes);
            }
        }
        catch (Exception)
        {
            // Malformed metadata (e.g. offsets that point outside the data) is ignored
        }
        return null;
    }

    /// <summary>
    /// Creates minimal Exif metadata (without the "Exif\0\0" marker) that only contains the resolution.
    /// </summary>
    public static byte[] CreateResolutionExif(double xDpi, double yDpi)
    {
        // Little-endian TIFF header (8 bytes), then an IFD with 3 entries (2 + 3 * 12 + 4 bytes), then the 2 rationals
        const int ifdOffset = 8;
        const int valuesOffset = ifdOffset + 2 + 3 * 12 + 4;
        var data = new byte[valuesOffset + 16];
        data[0] = data[1] = 0x49;
        BinaryPrimitives.WriteInt16LittleEndian(data.AsSpan(2), 42);
        BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(4), ifdOffset);
        BinaryPrimitives.WriteInt16LittleEndian(data.AsSpan(ifdOffset), 3);
        // Entries (sorted by tag): tag, type, count, value or offset
        WriteEntry(data, ifdOffset + 2, TAG_X_RESOLUTION, TYPE_RATIONAL, valuesOffset);
        WriteEntry(data, ifdOffset + 14, TAG_Y_RESOLUTION, TYPE_RATIONAL, valuesOffset + 8);
        WriteEntry(data, ifdOffset + 26, TAG_RESOLUTION_UNIT, TYPE_SHORT, 2 /* inches */);
        WriteRational(data, valuesOffset, xDpi);
        WriteRational(data, valuesOffset + 8, yDpi);
        return data;
    }

    private static EndianReader CreateReader(byte[] data, int offset) =>
        // 0x4949 ("II") = little-endian, 0x4D4D ("MM") = big-endian
        new((data[offset] == 0x49) != BitConverter.IsLittleEndian);

    private static void WriteEntry(byte[] data, int offset, short tag, short type, int value)
    {
        BinaryPrimitives.WriteInt16LittleEndian(data.AsSpan(offset), tag);
        BinaryPrimitives.WriteInt16LittleEndian(data.AsSpan(offset + 2), type);
        BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(offset + 4), 1);
        BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(offset + 8), value);
    }

    // Stores whole numbers exactly, and others with 3 decimal places. Invalid values (e.g. negative or NaN) are stored
    // as 0, which readers treat as no resolution.
    private static void WriteRational(byte[] data, int offset, double value)
    {
        value = value > 0 ? Math.Min(value, 1_000_000) : 0;
        bool whole = Math.Abs(value - Math.Round(value)) < 0.0005;
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(offset),
            (uint) Math.Round(whole ? value : value * 1000));
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(offset + 4), whole ? 1u : 1000u);
    }
}
