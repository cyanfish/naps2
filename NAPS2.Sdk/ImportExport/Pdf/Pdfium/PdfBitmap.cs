namespace NAPS2.ImportExport.Pdf.Pdfium;

public class PdfBitmap : NativePdfiumObject
{
    public const uint WHITE = uint.MaxValue;

    public static PdfBitmap CreateFromPointerBgr(int width, int height, IntPtr scan0, int stride)
    {
        return new PdfBitmap(
            Native.FPDFBitmap_CreateEx(width, height, PdfiumNativeLibrary.FPDFBitmap_BGR, scan0, stride));
    }

    private PdfBitmap(IntPtr handle) : base(handle)
    {
    }

    protected override void DisposeHandle()
    {
        Native.FPDFBitmap_Destroy(Handle);
    }

    public void FillRect(int x, int y, int width, int height, uint color)
    {
        Native.FPDFBitmap_FillRect(Handle, x, y, width, height, color);
    }

    public void RenderPage(PdfPage page, int x, int y, int width, int height)
    {
        int rotate = 0;
        int flags = PdfiumNativeLibrary.FPDF_PRINTING;
        Native.FPDF_RenderPageBitmap(Handle, page.Handle, x, y, width, height, rotate, flags);
    }
}