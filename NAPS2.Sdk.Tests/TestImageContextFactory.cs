using NAPS2.Images.Gdi;

namespace NAPS2.Sdk.Tests;

public static class TestImageContextFactory
{
    public static ImageContext Get(IPdfRenderer pdfRenderer = null)
    {
#if NET6_0_OR_GREATER
        if (OperatingSystem.IsMacOS())
        {
            return new NAPS2.Images.Mac.MacImageContext(pdfRenderer);
        }
        else
        {
            return new GdiImageContext(pdfRenderer);
        }
#else
        return new GdiImageContext(pdfRenderer);
#endif
    }
}