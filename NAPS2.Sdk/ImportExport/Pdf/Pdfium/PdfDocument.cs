namespace NAPS2.ImportExport.Pdf.Pdfium;

public class PdfDocument : NativePdfiumObject
{
    public static PdfDocument Load(string path, string? password = null)
    {
        return new PdfDocument(Native.FPDF_LoadDocument(path, password));
    }

    public static PdfDocument CreateNew()
    {
        return new PdfDocument(Native.FPDF_CreateNewDocument());
    }
    
    private PdfDocument(IntPtr handle) : base(handle)
    {
    }

    public int PageCount => Native.FPDF_GetPageCount(Handle);

    public PdfPage GetPage(int pageIndex)
    {
        return new PdfPage(Native.FPDF_LoadPage(Handle, pageIndex));
    }

    protected override void DisposeHandle()
    {
        Native.FPDF_CloseDocument(Handle);
    }
}