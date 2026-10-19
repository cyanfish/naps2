using System.Runtime.InteropServices;
using System.Text;
using NAPS2.Images.Bitwise;

namespace NAPS2.Platform.Windows;

/// <summary>
/// Loads icons from Windows files (e.g. exes) as images.
/// </summary>
internal static class WindowsIcons
{
    public static IMemoryImage? ExtractAssociatedIcon(ImageContext imageContext, string path)
    {
        ushort index = 0;
        var hIcon = Win32.ExtractAssociatedIcon(IntPtr.Zero, new StringBuilder(path, 260), ref index);
        return FromHIcon(imageContext, hIcon);
    }

    public static IMemoryImage? ExtractIcon(ImageContext imageContext, string path, int index)
    {
        // Returns S_FALSE if there's no icon
        return Win32.SHDefExtractIcon(path, index, 0, out var hIcon, IntPtr.Zero, 0) == 0
            ? FromHIcon(imageContext, hIcon)
            : null;
    }

    private static unsafe IMemoryImage? FromHIcon(ImageContext imageContext, IntPtr hIcon)
    {
        if (hIcon == IntPtr.Zero) return null;
        var info = default(Win32.IconInfo);
        try
        {
            // Monochrome icons have no color bitmap, so aren't supported
            if (!Win32.GetIconInfo(hIcon, out info) ||
                Win32.GetObject(info.hbmColor, Marshal.SizeOf<Win32.BitmapStruct>(), out var bitmap) == 0)
            {
                return null;
            }
            int width = bitmap.bmWidth, height = bitmap.bmHeight;
            var pixels = GetPixels(info.hbmColor, width, height);
            if (pixels == null) return null;
            if (!HasAlpha(pixels))
            {
                // Older icons use a mask for transparency (white = transparent)
                var mask = GetPixels(info.hbmMask, width, height);
                for (int i = 3; i < pixels.Length; i += 4)
                {
                    pixels[i] = mask == null || mask[i - 3] == 0 ? (byte) 255 : (byte) 0;
                }
            }

            var image = imageContext.Create(width, height, ImagePixelFormat.ARGB32);
            fixed (byte* ptr = pixels)
            {
                new CopyBitwiseImageOp().Perform((IntPtr) ptr, new PixelInfo(width, height, SubPixelType.Bgra), image);
            }
            return image;
        }
        finally
        {
            Win32.DeleteObject(info.hbmColor);
            Win32.DeleteObject(info.hbmMask);
            Win32.DestroyIcon(hIcon);
        }
    }

    /// <summary>
    /// Gets the bitmap's pixels as top-down BGRA.
    /// </summary>
    private static byte[]? GetPixels(IntPtr hBitmap, int width, int height)
    {
        var header = new Win32.BitmapInfoHeader
        {
            biSize = Marshal.SizeOf<Win32.BitmapInfoHeader>(),
            biWidth = width,
            biHeight = -height,
            biPlanes = 1,
            biBitCount = 32
        };
        var pixels = new byte[width * height * 4];
        var hdc = Win32.GetDC(IntPtr.Zero);
        try
        {
            return Win32.GetDIBits(hdc, hBitmap, 0, (uint) height, pixels, ref header, 0) == height ? pixels : null;
        }
        finally
        {
            Win32.ReleaseDC(IntPtr.Zero, hdc);
        }
    }

    private static bool HasAlpha(byte[] pixels)
    {
        for (int i = 3; i < pixels.Length; i += 4)
        {
            if (pixels[i] != 0) return true;
        }
        return false;
    }
}
