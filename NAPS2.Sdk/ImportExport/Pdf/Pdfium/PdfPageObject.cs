using System.Runtime.InteropServices;

namespace NAPS2.ImportExport.Pdf.Pdfium;

public class PdfPageObject : NativePdfiumObject
{
    private readonly PdfDocument _document;
    private readonly PdfPage? _page;
    private readonly bool _owned;

    internal PdfPageObject(IntPtr handle, PdfDocument document, PdfPage? page, bool owned) : base(handle)
    {
        _document = document;
        _page = page;
        _owned = owned;
    }

    public void SetBitmap(PdfBitmap bitmap)
    {
        if (!Native.FPDFImageObj_SetBitmap(IntPtr.Zero, 0, Handle, bitmap.Handle))
        {
            throw new Exception("Could not set bitmap");
        }
    }

    public void LoadJpegFileInline(Stream stream)
    {
        int GetBlock(IntPtr param, ulong position, IntPtr buffer, ulong size)
        {
            stream.Seek((long) position, SeekOrigin.Begin);
            // TODO: Can we skip the intermediate buffer somehow?
            var intermediateBuffer = new byte[size];
            stream.Read(intermediateBuffer, 0, (int) size);
            Marshal.Copy(intermediateBuffer, 0, buffer, (int) size);
            return 1;
        }

        PdfiumNativeLibrary.FPDF_FileAccess fileAccess = new()
        {
            m_FileLen = (ulong) stream.Length,
            m_GetBlock = GetBlock
        };
        IntPtr p = Marshal.AllocHGlobal(Marshal.SizeOf(fileAccess));
        Marshal.StructureToPtr(fileAccess, p, false);
        try
        {
            if (!Native.FPDFImageObj_LoadJpegFileInline(IntPtr.Zero, 0, Handle, p))
            {
                throw new Exception("Could not load jpeg");
            }
        }
        finally
        {
            Marshal.FreeHGlobal(p);
        }
    }

    public bool IsImage => Native.FPDFPageObj_GetType(Handle) == PdfiumNativeLibrary.FPDF_PAGEOBJ_IMAGE;

    public bool IsText => Native.FPDFPageObj_GetType(Handle) == PdfiumNativeLibrary.FPDF_PAGEOBJ_TEXT;

    public PdfMatrix Matrix
    {
        get
        {
            if (!Native.FPDFPageObj_GetMatrix(Handle, out var matrix))
            {
                throw new Exception("Could not get matrix");
            }
            return matrix;
        }
        set
        {
            if (!Native.FPDFPageObj_SetMatrix(Handle, ref value))
            {
                throw new Exception("Could not set matrix");
            }
        }
    }

    public uint GetStrokeAlpha()
    {
        // TODO: Maybe fill color? Or something else to get the text color
        if (!Native.FPDFPageObj_GetStrokeColor(Handle, out var r, out var g, out var b, out var a))
        {
            throw new Exception("Could not get stroke color");
        }
        return a;
    }

    public PdfBitmap GetBitmap()
    {
        return new PdfBitmap(Native.FPDFImageObj_GetBitmap(Handle));
    }

    public PdfBitmap GetRenderedBitmap()
    {
        return new PdfBitmap(
            Native.FPDFImageObj_GetRenderedBitmap(_document.Handle, _page?.Handle ?? IntPtr.Zero, Handle));
    }

    protected override void DisposeHandle()
    {
        if (_owned)
        {
            Native.FPDFPageObj_Destroy(Handle);
        }
    }
}