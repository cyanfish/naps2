using System.Runtime.InteropServices;

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

    public PdfPageObject NewImage()
    {
        return new PdfPageObject(Native.FPDFPageObj_NewImageObj(Handle));
    }

    public PdfPage NewPage(double width, double height)
    {
        return new PdfPage(Native.FPDFPage_New(Handle, int.MaxValue, width, height));
    }

    public void Save(string path)
    {
        using var stream = new FileStream(path, FileMode.Create);
        int WriteBlock(IntPtr self, IntPtr data, ulong size)
        {
            var buffer = new byte[size];
            Marshal.Copy(data, buffer, 0, (int) size);
            stream.Write(buffer, 0, (int) size);
            return 1;
        }
        
        PdfiumNativeLibrary.FPDF_FileWrite fileWrite = new()
        {
            WriteBlock = WriteBlock
        };
        IntPtr p = Marshal.AllocHGlobal(Marshal.SizeOf(fileWrite));
        Marshal.StructureToPtr(fileWrite, p, false);
        try
        {
            if (!Native.FPDF_SaveAsCopy(Handle, p, PdfiumNativeLibrary.FPDF_NOINCREMENTAL))
            {
                throw new IOException("Failed to save PDF");
            }
        }
        finally
        {
            Marshal.FreeHGlobal(p);
        }
    }
}