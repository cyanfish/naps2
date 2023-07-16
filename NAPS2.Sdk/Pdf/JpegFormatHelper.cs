namespace NAPS2.Pdf;

internal static class JpegFormatHelper
{
    // JPEG format doc: https://github.com/corkami/formats/blob/master/image/jpeg.md

    public static JpegHeader? ReadHeader(FileStream stream)
    {
        var sig1 = stream.ReadByte();
        var sig2 = stream.ReadByte();
        if (sig1 != 0xFF || sig2 != 0xD8) return null;
        var header = new JpegHeader(0, 0, 0, 0, 0);
        bool isJfif = false;
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
                isJfif = true;
                var units = buf[7];
                if (units == 1) // Pixels per inch
                {
                    header = header with
                    {
                        HorizontalDpi = buf[8] * 256 + buf[9],
                        VerticalDpi = buf[10] * 256 + buf[11]
                    };
                }
                else if (units == 2) // Pixels per cm
                {
                    header = header with
                    {
                        HorizontalDpi = (buf[8] * 256 + buf[9]) * 2.54,
                        VerticalDpi = (buf[10] * 256 + buf[11]) * 2.54
                    };
                }
            }
            if (type == 0xC0 && len >= 6) // Start of frame
            {
                // JPEGs can come with several header varieties: JFIF-only, EXIF-only, or JFIF+EXIF.
                // As long as we have a JFIF header we should have accurate resolution information.
                // If it's EXIF-only we're not currently able to parse that.
                if (!isJfif) return null;
                return header with
                {
                    Height = buf[1] * 256 + buf[2],
                    Width = buf[3] * 256 + buf[4],
                    NumComponents = buf[5]
                };
            }
        }
    }

    public record JpegHeader(double HorizontalDpi, double VerticalDpi, int Width, int Height, int NumComponents);
}