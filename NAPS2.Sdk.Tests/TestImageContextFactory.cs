namespace NAPS2.Sdk.Tests;

public static class TestImageContextFactory
{
    public static ImageContext Get()
    {
        // TODO: Add IMAGESHARP/WPF compiler variables and have special test commands that run in CI on at least one platform
#if MAC
        return new NAPS2.Images.Mac.MacImageContext();
#elif LINUX
        return new NAPS2.Images.Gtk.GtkImageContext();
#else
        return new NAPS2.Images.Gdi.GdiImageContext();
#endif
    }
}