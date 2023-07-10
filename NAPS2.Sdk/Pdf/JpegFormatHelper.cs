namespace NAPS2.Pdf;

internal static class JpegFormatHelper
{
    // JPEG format doc: https://github.com/corkami/formats/blob/master/image/jpeg.md

    public static int ReadNumComponents(FileStream stream)
    {
        var sig1 = stream.ReadByte();
        var sig2 = stream.ReadByte();
        if (sig1 != 0xFF || sig2 != 0xD8) return -1;
        while (true)
        {
            var marker = stream.ReadByte();
            if (marker != 0xFF) return -1;
            var type = stream.ReadByte();
            if (type == -1) return -1;
            var len = stream.ReadByte() * 256 + stream.ReadByte() - 2;
            if (len <= 0) return -1;
            var buf = new byte[len];
            if (stream.Read(buf, 0, len) < len) return -1;
            if (type == 0xC0 && len >= 6) // Start of frame
            {
                return buf[5];
            }
        }
    }
}