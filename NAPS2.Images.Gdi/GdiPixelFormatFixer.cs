using System.Drawing;
using System.Drawing.Imaging;

namespace NAPS2.Images.Gdi;

/// <summary>
/// Ensures that bitmaps use a standard pixel format/palette.
/// </summary>
internal static class GdiPixelFormatFixer
{
    public static bool MaybeFixPixelFormat(ref Bitmap bitmap)
    {
        switch (bitmap.PixelFormat)
        {
            case PixelFormat.Format1bppIndexed when IsPalette(bitmap, Color.Black, Color.White):
                return false;
            case PixelFormat.Format1bppIndexed when IsPalette(bitmap, Color.White, Color.Black):
                InvertPalette(bitmap);
                return true;
            case PixelFormat.Format1bppIndexed:
                Redraw(ref bitmap, PixelFormat.Format24bppRgb);
                return true;
            case PixelFormat.Format8bppIndexed:
            {
                if (!Is8BitGrayscalePalette(bitmap))
                {
                    Redraw(ref bitmap, PixelFormat.Format24bppRgb);
                    return true;
                }
                return false;
            }
            case PixelFormat.Format24bppRgb:
            case PixelFormat.Format32bppArgb:
                return false;
            default:
                Redraw(
                    ref bitmap,
                    Image.IsAlphaPixelFormat(bitmap.PixelFormat)
                        ? PixelFormat.Format32bppArgb
                        : PixelFormat.Format24bppRgb);
                return true;
        }
    }

    private static bool IsPalette(Bitmap bitmap, params Color[] colors)
    {
        if (colors.Length != bitmap.Palette.Entries.Length)
        {
            return false;
        }
        for (int i = 0; i < colors.Length; i++)
        {
            if (colors[i].ToArgb() != bitmap.Palette.Entries[i].ToArgb())
            {
                return false;
            }
        }
        return true;
    }

    private static bool Is8BitGrayscalePalette(Bitmap bitmap)
    {
        for (int i = 0; i < 256; i++)
        {
            if (bitmap.Palette.Entries[i].ToArgb() != Color.FromArgb(i, i, i).ToArgb())
            {
                return false;
            }
        }
        return true;
    }

    private static unsafe void InvertPalette(Bitmap bitmap)
    {
        var p = bitmap.Palette;
        p.Entries[0] = Color.Black;
        p.Entries[1] = Color.White;
        bitmap.Palette = p;
        using var lockState = GdiImageLockState.Create(bitmap, LockMode.ReadWrite, out var data);
        for (int i = 0; i < data.h; i++)
        {
            var row = data.ptr + i * data.stride;
            for (int j = 0; j < data.stride; j++)
            {
                var b = row + j;
                *b = (byte) (~*b & 0xFF);
            }
        }
    }

    private static void Redraw(ref Bitmap bitmap, PixelFormat newPixelFormat)
    {
        var newBitmap = new Bitmap(bitmap.Width, bitmap.Height, newPixelFormat);
        using var g = Graphics.FromImage(newBitmap);
        g.DrawImage(bitmap, 0, 0, bitmap.Width, bitmap.Height);
        bitmap.Dispose();
        bitmap = newBitmap;
    }
}