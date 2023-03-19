namespace NAPS2.Pdf.Pdfium;

internal class PdfBitmap : NativePdfiumObject
{
    public const uint BLACK = 0;
    public const uint WHITE = uint.MaxValue;

    public static PdfBitmap CreateFromPointerBgr(int width, int height, IntPtr scan0, int stride)
    {
        return new PdfBitmap(
            Native.FPDFBitmap_CreateEx(width, height, PdfiumNativeLibrary.FPDFBitmap_BGR, scan0, stride));
    }

    public static PdfBitmap CreateFromPointer(int width, int height, IntPtr scan0, int stride, int format)
    {
        return new PdfBitmap(
            Native.FPDFBitmap_CreateEx(width, height, format, scan0, stride));
    }

    internal PdfBitmap(IntPtr handle) : base(handle)
    {
    }

    public int Width => Native.FPDFBitmap_GetWidth(Handle);
    
    public int Height => Native.FPDFBitmap_GetHeight(Handle);
    
    public int Stride => Native.FPDFBitmap_GetStride(Handle);

    public int Format => Native.FPDFBitmap_GetFormat(Handle);

    public IntPtr Buffer => Native.FPDFBitmap_GetBuffer(Handle);

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