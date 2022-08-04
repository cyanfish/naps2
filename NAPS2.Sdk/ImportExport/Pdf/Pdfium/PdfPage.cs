namespace NAPS2.ImportExport.Pdf.Pdfium;

public class PdfPage : NativePdfiumObject
{

    internal PdfPage(IntPtr handle, PdfDocument document, int pageIndex) : base(handle)
    {
        Document = document;
        PageIndex = pageIndex;
    }

    public PdfDocument Document { get; }

    public int PageIndex { get; }
    
    public float Width => Native.FPDF_GetPageWidthF(Handle);
    
    public float Height => Native.FPDF_GetPageHeightF(Handle);

    public bool HasTransparency => Native.FPDFPage_HasTransparency(Handle);

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
    
    public void RemoveObject(PdfPageObject pageObject)
    {
        if (!Native.FPDFPage_RemoveObject(Handle, pageObject.Handle))
        {
            throw new Exception("Could not remove page object");
        }
        pageObject.SetAlreadyDisposed();
    }

    public PdfPageObject GetObject(int index)
    {
        var pageObj = new PdfPageObject(Native.FPDFPage_GetObject(Handle, index),Document, this, false);
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