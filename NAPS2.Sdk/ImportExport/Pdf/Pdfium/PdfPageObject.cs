namespace NAPS2.ImportExport.Pdf.Pdfium;

public class PdfPageObject : NativePdfiumObject
{
    internal PdfPageObject(IntPtr handle) : base(handle)
    {
    }

    public void SetBitmap(PdfBitmap bitmap)
    {
        if (!Native.FPDFImageObj_SetBitmap(IntPtr.Zero, 0, Handle, bitmap.Handle))
        {
            throw new Exception("Could not set bitmap");
        }
    }

    public void Transform(double a, double b, double c, double d, double e, double f)
    {
        Native.FPDFPageObj_Transform(Handle, a, b, c, d, e, f);
    }

    protected override void DisposeHandle()
    {
        Native.FPDFPageObj_Destroy(Handle);
    }
}