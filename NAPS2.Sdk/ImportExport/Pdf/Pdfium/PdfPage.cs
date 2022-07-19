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

    public void InsertObject(PdfPageObject pageObject)
    {
        Native.FPDFPage_InsertObject(Handle, pageObject.Handle);
        pageObject.SetAlreadyDisposed();
    }

    public void GenerateContent()
    {
        Native.FPDFPage_GenerateContent(Handle);
    }

    protected override void DisposeHandle()
    {
        Native.FPDF_ClosePage(Handle);
    }
}