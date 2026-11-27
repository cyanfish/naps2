namespace NAPS2.ImportExport;

public static class JpegFormatHelper
{
    // JPEG format doc: https://github.com/corkami/formats/blob/master/image/jpeg.md

    public static JpegHeader? ReadHeader(Stream stream)
    {
        var sig1 = stream.ReadByte();
        var sig2 = stream.ReadByte();
        if (sig1 != 0xFF || sig2 != 0xD8) return null;
        var header = new JpegHeader(0, 0, 0, 0, 0, false, false, false);
        while (true)
        {
            var marker = stream.ReadByte();
            if (marker != 0xFF) return null;
            var type = stream.ReadByte();
            if (type == -1) return null;
            var len = stream.ReadByte() * 256 + stream.ReadByte() - 2;
            if (len <= 0) return null;
            var buf = new byte[len];
            if (stream.Read(buf, 0, len) < len) return null;
            if (type == 0xE0 && len >= 12 && buf is [0x4A, 0x46, 0x49, 0x46, 0x00, ..]) // Application data (JFIF)
            {
                header = ReadJfif(buf, header);
            }
            if (type == 0xE1 && len >= 12 && buf is [0x45, 0x78, 0x69, 0x66, 0x00, 0x00, ..]) // Application data (EXIF)
            {
                header = ReadExif(buf, header);
            }
            // Start of frame. 0xC0 is baseline, and the rest of the 0xC0-0xCF range is for other encodings (e.g. 0xC2
            // is progressive), except for 0xC4/0xC8/0xCC which aren't frame markers.
            if (type is >= 0xC0 and <= 0xCF and not (0xC4 or 0xC8 or 0xCC) && len >= 6)
            {
                return header with
                {
                    Height = buf[1] * 256 + buf[2],
                    Width = buf[3] * 256 + buf[4],
                    NumComponents = buf[5],
                    IsBaseline = type == 0xC0
                };
            }
        }
    }

    private static JpegHeader ReadJfif(byte[] buf, JpegHeader header)
    {
        header = header with { HasJfifHeader = true };
        var units = buf[7];
        var hRes = buf[8] * 256 + buf[9];
        var vRes = buf[10] * 256 + buf[11];
        if (units == 0 && hRes > 10 && vRes > 10) // Unspecified units but assume pixels per inch
        {
            header = header with
            {
                HorizontalDpi = hRes,
                VerticalDpi = vRes
            };
        }
        else if (units == 1) // Pixels per inch
        {
            header = header with
            {
                HorizontalDpi = hRes,
                VerticalDpi = vRes
            };
        }
        else if (units == 2) // Pixels per cm
        {
            header = header with
            {
                HorizontalDpi = hRes * 2.54,
                VerticalDpi = vRes * 2.54
            };
        }
        return header;
    }

    private static JpegHeader ReadExif(byte[] buf, JpegHeader header)
    {
        // The segment starts with "Exif\0\0", followed by the Exif metadata
        if (!ExifHelper.IsExif(buf, 6))
        {
            return header;
        }
        header = header with { HasExifHeader = true };
        if (ExifHelper.ReadResolution(buf, 6) is var (xDpi, yDpi))
        {
            header = header with { HorizontalDpi = xDpi, VerticalDpi = yDpi };
        }
        return header;
    }

    public record JpegHeader(
        double HorizontalDpi,
        double VerticalDpi,
        int Width,
        int Height,
        int NumComponents,
        bool HasJfifHeader,
        bool HasExifHeader,
        bool IsBaseline);
}