namespace NAPS2.Pdf.Pdfium;

internal class PdfFont : NativePdfiumObject
{
    public PdfFont(IntPtr handle) : base(handle)
    {
    }

    protected override void DisposeHandle()
    {
        Native.FPDFFont_Close(Handle);
    }
}