using System.Windows.Media;
using System.Windows.Media.Imaging;
using NAPS2.Images.Bitwise;

namespace NAPS2.Images.Wpf;

/// <summary>
/// Ensures that bitmaps use a standard pixel format/palette.
/// </summary>
internal static class WpfPixelFormatFixer
{
    public static bool MaybeFixPixelFormat(ref WriteableBitmap bitmap)
    {
        if (bitmap.Format.BitsPerPixel == 1)
        {
            if (bitmap.Palette?.Colors[0] == Colors.White && bitmap.Palette?.Colors[1] == Colors.Black)
            {
                InvertPalette(ref bitmap);
                return true;
            }
        }
        return false;
    }

    private static void InvertPalette(ref WriteableBitmap bitmap)
    {
        int w = bitmap.PixelWidth;
        int h = bitmap.PixelHeight;

        var src = bitmap.BackBuffer;
        var srcInfo = new PixelInfo(w, h, SubPixelType.InvertedBit, bitmap.BackBufferStride);

        var newBitmap = new WriteableBitmap(w, h, bitmap.DpiX, bitmap.DpiY, PixelFormats.BlackWhite, null);
        var dst = newBitmap.BackBuffer;
        var dstInfo = new PixelInfo(w, h, SubPixelType.Bit, newBitmap.BackBufferStride);

        new CopyBitwiseImageOp().Perform(src, srcInfo, dst, dstInfo);
        newBitmap.Freeze();
        bitmap = newBitmap;
    }
}