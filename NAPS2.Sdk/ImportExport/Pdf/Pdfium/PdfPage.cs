namespace NAPS2.ImportExport.Pdf.Pdfium;

public class PdfPage : NativePdfiumObject
{
    private readonly PdfDocument _document;

    internal PdfPage(IntPtr handle, PdfDocument document) : base(handle)
    {
        _document = document;
    }
    
    public float Width => Native.FPDF_GetPageWidthF(Handle);
    
    public float Height => Native.FPDF_GetPageHeightF(Handle);

    public PdfText GetText()
    {
        return new PdfText(Native.FPDFText_LoadPage(Handle));
    }

    public int ObjectCount => Native.FPDFPage_CountObjects(Handle);

    public void InsertObject(PdfPageObject pageObject)
    {
        Native.FPDFPage_InsertObject(Handle, pageObject.Handle);
        pageObject.SetAlreadyDisposed();
    }

    public PdfPageObject GetObject(int index)
    {
        var pageObj = new PdfPageObject(Native.FPDFPage_GetObject(Handle, index),_document, this, false);
        return pageObj;
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