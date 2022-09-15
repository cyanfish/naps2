using System.Runtime.InteropServices;

namespace NAPS2.Images.Gtk;

public static class LibTiff
{
    // TODO: String marshalling?
    [DllImport("libtiff.so.5")]
    public static extern IntPtr TIFFOpen(string filename, string mode);

    [DllImport("libtiff.so.5")]
    public static extern IntPtr TIFFClose(IntPtr tiff);

    [DllImport("libtiff.so.5")]
    public static extern int TIFFReadDirectory(IntPtr tiff);

    [DllImport("libtiff.so.5")]
    public static extern int TIFFGetField(IntPtr tiff, int tag, out int field);

    [DllImport("libtiff.so.5")]
    public static extern int TIFFReadRGBAImage(IntPtr tiff, int w, int h, IntPtr raster, int stopOnError);

    // TODO: For streams
    // https://linux.die.net/man/3/tiffclientopen
}