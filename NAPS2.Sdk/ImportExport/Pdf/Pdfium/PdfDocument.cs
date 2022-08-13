using System.Runtime.InteropServices;
using System.Text;

namespace NAPS2.ImportExport.Pdf.Pdfium;

// TODO: Use PdfiumException (with a message, defaulting to unknown error code for the property) instead of other exception types
public class PdfDocument : NativePdfiumObject
{
    public static PdfDocument Load(string path, string? password = null)
    {
        return new PdfDocument(Native.FPDF_LoadDocument(path, password), PlatformCompat.System.FileReadLock(path));
    }

    public static PdfDocument Load(IntPtr buffer, int length, string? password = null)
    {
        return new PdfDocument(Native.FPDF_LoadMemDocument(buffer, length, password));
    }

    public static PdfDocument CreateNew()
    {
        return new PdfDocument(Native.FPDF_CreateNewDocument());
    }

    private readonly IDisposable? _readLock;

    private PdfDocument(IntPtr handle, IDisposable? readLock = null) : base(handle)
    {
        _readLock = readLock;
    }

    public int PageCount => Native.FPDF_GetPageCount(Handle);

    public PdfPage GetPage(int pageIndex)
    {
        return new PdfPage(Native.FPDF_LoadPage(Handle, pageIndex), this, pageIndex);
    }

    public void DeletePage(int pageIndex)
    {
        Native.FPDFPage_Delete(Handle, pageIndex);
    }

    public PdfPageObject NewImage()
    {
        return new PdfPageObject(Native.FPDFPageObj_NewImageObj(Handle), this, null, true);
    }

    public PdfPage NewPage(double width, double height)
    {
        return new PdfPage(Native.FPDFPage_New(Handle, int.MaxValue, width, height), this, -1);
    }

    public void ImportPages(PdfDocument sourceDoc, string? pageRange = null, int insertIndex = 0)
    {
        if (!Native.FPDF_ImportPages(Handle, sourceDoc.Handle, pageRange, insertIndex))
        {
            throw new Exception("Could not import PDF pages");
        }
    }

    public void ImportPage(PdfPage page)
    {
        ImportPages(page.Document, (page.PageIndex + 1).ToString(), PageCount);
    }

    public string GetMetaText(string tag)
    {
        var length = Native.FPDF_GetMetaText(Handle, tag, null, (IntPtr) 0);
        var buffer = new byte[(int) length];
        Native.FPDF_GetMetaText(Handle, tag, buffer, length);
        return Encoding.Unicode.GetString(buffer, 0, buffer.Length - 2);
    }

    public void Save(string path)
    {
        using var stream = new FileStream(path, FileMode.Create);
        Save(stream);
    }

    public void Save(Stream stream)
    {
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

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _readLock?.Dispose();
        }
    }

    protected override void DisposeHandle()
    {
        Native.FPDF_CloseDocument(Handle);
    }
}