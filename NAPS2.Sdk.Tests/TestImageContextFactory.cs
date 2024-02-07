namespace NAPS2.Sdk.Tests;

public static class TestImageContextFactory
{
    public static ImageContext Get()
    {
        // TODO: For now we use ImageSharp on net6-windows for coverage. But eventually we'll need to do something
        // more comprehensive, i.e. set up some IMAGESHARP/SKIA compiler variables and have special test commands.
#if MAC
        return new NAPS2.Images.Mac.MacImageContext();
#elif LINUX
        return new NAPS2.Images.Gtk.GtkImageContext();
#elif NET6_0_OR_GREATER
        return new NAPS2.Images.ImageSharp.ImageSharpImageContext();
#else
        return new NAPS2.Images.Gdi.GdiImageContext();
#endif
    }
}