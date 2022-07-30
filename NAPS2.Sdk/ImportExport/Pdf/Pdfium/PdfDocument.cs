using System.Runtime.InteropServices;

namespace NAPS2.ImportExport.Pdf.Pdfium;

public class PdfDocument : NativePdfiumObject
{
    public static PdfDocument Load(string path, string? password = null)
    {
        return new PdfDocument(Native.FPDF_LoadDocument(path, password));
    }

    public static PdfDocument Load(IntPtr buffer, int length, string? password = null)
    {
        return new PdfDocument(Native.FPDF_LoadMemDocument(buffer, length, password));
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
        return new PdfPage(Native.FPDF_LoadPage(Handle, pageIndex), this);
    }

    protected override void DisposeHandle()
    {
        Native.FPDF_CloseDocument(Handle);
    }

    public PdfPageObject NewImage()
    {
        return new PdfPageObject(Native.FPDFPageObj_NewImageObj(Handle), this, null, true);
    }

    public PdfPage NewPage(double width, double height)
    {
        return new PdfPage(Native.FPDFPage_New(Handle, int.MaxValue, width, height), this);
    }

    public void Save(string path)
    {
        using var stream = new FileStream(path, FileMode.Create);

        int WriteBlock(IntPtr self, IntPtr data, IntPtr size)
        {
            var buffer = new byte[(int) size];
            Marshal.Copy(data, buffer, 0, (int) size);
            stream.Write(buffer, 0, (int) size);
            return 1;
        }

        PdfiumNativeLibrary.FPDF_FileWrite fileWrite = new()
        {
            version = 1,
            WriteBlock = WriteBlock
        };
        if (!Native.FPDF_SaveAsCopy(Handle, ref fileWrite, PdfiumNativeLibrary.FPDF_NOINCREMENTAL))
        {
            throw new IOException("Failed to save PDF");
        }
    }
}