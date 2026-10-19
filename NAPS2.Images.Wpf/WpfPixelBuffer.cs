using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32.SafeHandles;

namespace NAPS2.Images.Wpf;

/// <summary>
/// Native memory for a WpfImage's pixel data, along with its layout.
///
/// This is used for storage instead of a WriteableBitmap, as a WriteableBitmap is double-buffered (using twice the
/// memory), its memory is only released when it's garbage collected, and it's tied to the thread that created it. As a
/// SafeHandle, the memory is still released if the image isn't disposed.
/// </summary>
internal sealed class WpfPixelBuffer : SafeHandleZeroOrMinusOneIsInvalid
{
    private const uint LMEM_FIXED = 0x0;
    private const uint LPTR = 0x40;

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr LocalAlloc(uint uFlags, UIntPtr uBytes);

    public static WpfPixelBuffer Allocate(int width, int height, PixelFormat format, bool zeroed)
    {
        long stride = ((long) width * format.BitsPerPixel + 31) / 32 * 4;
        long size = stride * height;
        if (size > int.MaxValue)
        {
            throw new NotSupportedException("Images larger than 2GB are not supported.");
        }
        // Equivalent to Marshal.AllocHGlobal, but can also zero the memory
        var ptr = LocalAlloc(zeroed ? LPTR : LMEM_FIXED, (UIntPtr) (ulong) size);
        if (ptr == IntPtr.Zero)
        {
            throw new OutOfMemoryException();
        }
        var buffer = new WpfPixelBuffer(ptr, width, height, format, (int) stride);
        // Let the GC know about the native memory, so undisposed images are still collected reasonably promptly
        GC.AddMemoryPressure(buffer.Size);
        return buffer;
    }

    public static WpfPixelBuffer FromSource(BitmapSource source)
    {
        var format = GetStorageFormat(source);
        if (format != source.Format)
        {
            source = new FormatConvertedBitmap(source, format, null, 0);
        }
        var buffer = Allocate(source.PixelWidth, source.PixelHeight, format, false);
        try
        {
            source.CopyPixels(Int32Rect.Empty, buffer.Ptr, (int) buffer.Size, buffer.Stride);
        }
        catch (Exception)
        {
            buffer.Dispose();
            throw;
        }
        return buffer;
    }

    private static PixelFormat GetStorageFormat(BitmapSource source)
    {
        var format = source.Format;
        if (format == PixelFormats.Gray8 || format == PixelFormats.Bgr24 || format == PixelFormats.Bgra32 ||
            format == PixelFormats.BlackWhite)
        {
            // Formats we store as-is
            return format;
        }
        if (format.BitsPerPixel == 1)
        {
            // e.g. Indexed1, which may have an inverted palette
            return PixelFormats.BlackWhite;
        }
        if (source.Palette is { } palette)
        {
            // Indexed formats: use the smallest format that can hold the palette's colors
            if (palette.Colors.Any(c => c.A != 255)) return PixelFormats.Bgra32;
            return palette.Colors.All(c => c.R == c.G && c.G == c.B) ? PixelFormats.Gray8 : PixelFormats.Bgr24;
        }
        return format.Masks.Count switch
        {
            1 => PixelFormats.Gray8,
            // Cmyk32 has 4 channels but no alpha
            4 when format != PixelFormats.Cmyk32 => PixelFormats.Bgra32,
            // Includes Bgr32, as its 4th byte is padding rather than alpha (and would be misread as transparency)
            _ => PixelFormats.Bgr24
        };
    }

    private WpfPixelBuffer(IntPtr ptr, int width, int height, PixelFormat format, int stride) : base(true)
    {
        SetHandle(ptr);
        Width = width;
        Height = height;
        Format = format;
        Stride = stride;
    }

    public IntPtr Ptr => handle;

    public int Width { get; }

    public int Height { get; }

    public PixelFormat Format { get; }

    public int Stride { get; }

    public long Size => (long) Stride * Height;

    public WpfPixelBuffer Clone()
    {
        var clone = Allocate(Width, Height, Format, false);
        unsafe
        {
            Buffer.MemoryCopy((void*) Ptr, (void*) clone.Ptr, clone.Size, Size);
        }
        return clone;
    }

    public BitmapSource ToBitmapSource(double xDpi, double yDpi)
    {
        var source = BitmapSource.Create(Width, Height, xDpi > 0 ? xDpi : 96, yDpi > 0 ? yDpi : 96, Format, null, Ptr,
            (int) Size, Stride);
        source.Freeze();
        return source;
    }

    protected override bool ReleaseHandle()
    {
        Marshal.FreeHGlobal(handle);
        GC.RemoveMemoryPressure(Size);
        return true;
    }
}
