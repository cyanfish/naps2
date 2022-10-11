namespace NAPS2.Sdk.Tests;

public static class TestImageContextFactory
{
    public static ImageContext Get(IPdfRenderer pdfRenderer = null)
    {
#if MAC
        return new NAPS2.Images.Mac.MacImageContext(pdfRenderer);
#elif LINUX
        return new NAPS2.Images.Gtk.GtkImageContext(pdfRenderer);
#else
#if NET6_0_OR_GREATER
        if (!OperatingSystem.IsWindowsVersionAtLeast(7)) throw new InvalidOperationException();
#endif
        return new NAPS2.Images.Gdi.GdiImageContext(pdfRenderer);
#endif
    }
}