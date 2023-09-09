namespace NAPS2.Pdf;

internal static class JpegFormatHelper
{
    // JPEG format doc: https://github.com/corkami/formats/blob/master/image/jpeg.md

    public static JpegHeader? ReadHeader(Stream stream)
    {
        var sig1 = stream.ReadByte();
        var sig2 = stream.ReadByte();
        if (sig1 != 0xFF || sig2 != 0xD8) return null;
        var header = new JpegHeader(0, 0, 0, 0, 0, false, false);
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
            }
            if (type == 0xE1 && len >= 12 && buf is [0x45, 0x78, 0x69, 0x66, 0x00, 0x00, ..]) // Application data (EXIF)
            {
                // https://docs.fileformat.com/image/exif/

                // 0x4949 = little-endian, 0x4d4d = big-endian
                var flipEnd = (buf[6] == 0x49 && buf[7] == 0x49) != BitConverter.IsLittleEndian;
                var reader = new EndianReader(flipEnd);

                var number = reader.ReadInt16(buf, 8);

                if (number == 42)
                {
                    header = header with { HasExifHeader = true };

                    var ifdOffset = reader.ReadInt32(buf, 10);
                    var dirCount = reader.ReadInt16(buf, ifdOffset + 6);

                    double xRes = 0;
                    double yRes = 0;
                    int resUnit = 0;

                    for (int i = 0; i < dirCount; i++)
                    {
                        var offset = ifdOffset + 8 + i * 12;

                        var tag = reader.ReadInt16(buf, offset);

                        if (tag == 282)
                        {
                            var valueOffset = reader.ReadInt32(buf, offset + 8);
                            var num = reader.ReadInt32(buf, valueOffset + 6);
                            var den = reader.ReadInt32(buf, valueOffset + 10);
                            xRes = num / (double) den;
                        }
                        if (tag == 283)
                        {
                            var valueOffset = reader.ReadInt32(buf, offset + 8);
                            var num = reader.ReadInt32(buf, valueOffset + 6);
                            var den = reader.ReadInt32(buf, valueOffset + 10);
                            yRes = num / (double) den;
                        }
                        if (tag == 296)
                        {
                            resUnit = reader.ReadInt16(buf, offset + 8);
                        }
                    }

                    if (xRes > 0 && yRes > 0)
                    {
                        if (resUnit == 3) // cm
                        {
                            header = header with
                            {
                                HorizontalDpi = xRes * 2.54,
                                VerticalDpi = yRes * 2.54
                            };
                        }
                        else // inch
                        {
                            header = header with
                            {
                                HorizontalDpi = xRes,
                                VerticalDpi = yRes
                            };
                        }
                    }
                }
            }
            if (type == 0xC0 && len >= 6) // Start of frame
            {
                return header with
                {
                    Height = buf[1] * 256 + buf[2],
                    Width = buf[3] * 256 + buf[4],
                    NumComponents = buf[5]
                };
            }
        }
    }

    public record JpegHeader(double HorizontalDpi, double VerticalDpi, int Width, int Height, int NumComponents,
        bool HasJfifHeader, bool HasExifHeader);
}