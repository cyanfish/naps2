namespace NAPS2.Sdk.Tests;

public static class TestImageContextFactory
{
    public static ImageContext Get()
    {
        return Environment.GetEnvironmentVariable("NAPS2_TEST_IMAGES") switch
        {
#if WINDOWS
            "gdi" => new NAPS2.Images.Gdi.GdiImageContext(),
            "wpf" => new NAPS2.Images.Wpf.WpfImageContext(),
#endif
            "skia" => new NAPS2.Images.Skia.SkiaImageContext(),
#if MAC
            "mac" => new NAPS2.Images.Mac.MacImageContext(),
#endif
#if LINUX
            "gtk" or "gdk" or "linux" => new NAPS2.Images.Gtk.GtkImageContext(),
#endif
            _ =>
#if MAC
                new NAPS2.Images.Mac.MacImageContext()
#elif LINUX
                new NAPS2.Images.Gtk.GtkImageContext()
#elif WINDOWS
                new NAPS2.Images.Gdi.GdiImageContext()
#else
                new NAPS2.Images.Skia.SkiaImageContext()
#endif
        };
    }
}