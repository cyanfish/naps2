namespace NAPS2.ImportExport.Pdf.Pdfium;

public class PdfPage : NativePdfiumObject
{
    internal PdfPage(IntPtr handle) : base(handle)
    {
    }
    
    public double Width => Native.FPDF_GetPageWidth(Handle);
    
    public double Height => Native.FPDF_GetPageHeight(Handle);

    public PdfText GetText()
    {
        return new PdfText(Native.FPDFText_LoadPage(Handle));
    }

    protected override void DisposeHandle()
    {
        Native.FPDF_ClosePage(Handle);
    }
}