using System.Runtime.InteropServices;
using System.Text;

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

    public void LoadJpegFileInline(MemoryStream stream)
    {
        int GetBlock(IntPtr param, IntPtr position, IntPtr buffer, IntPtr size)
        {
            var sourceBuffer = stream.GetBuffer();
            Marshal.Copy(sourceBuffer, (int) position, buffer, (int) size);
            return 1;
        }

        PdfiumNativeLibrary.FPDF_FileAccess fileAccess = new()
        {
            m_FileLen = (IntPtr) stream.Length,
            m_GetBlock = GetBlock
        };
        if (!Native.FPDFImageObj_LoadJpegFileInline(IntPtr.Zero, 0, Handle, ref fileAccess))
        {
            throw new Exception("Could not load jpeg");
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

    public (uint r, uint g, uint b, uint a) GetStrokeColor()
    {
        // TODO: Maybe fill color? Or something else to get the text color
        if (!Native.FPDFPageObj_GetStrokeColor(Handle, out var r, out var g, out var b, out var a))
        {
            throw new Exception("Could not get stroke color");
        }
        return (r, g, b, a);
    }

    public (uint r, uint g, uint b, uint a) GetFillColor()
    {
        if (!Native.FPDFPageObj_GetFillColor(Handle, out var r, out var g, out var b, out var a))
        {
            throw new Exception("Could not get fill color");
        }
        return (r, g, b, a);
    }

    public string GetText(PdfText pageText)
    {
        var length = Native.FPDFTextObj_GetText(Handle, pageText.Handle, null, (IntPtr) 0);
        var buffer = new byte[(int) length];
        Native.FPDFTextObj_GetText(Handle, pageText.Handle, buffer, length);
        return Encoding.Unicode.GetString(buffer);
    }

    public TextRenderMode TextRenderMode => (TextRenderMode) Native.FPDFTextObj_GetTextRenderMode(Handle);

    public PdfBitmap GetBitmap()
    {
        return new PdfBitmap(Native.FPDFImageObj_GetBitmap(Handle));
    }

    public PdfBitmap GetRenderedBitmap()
    {
        return new PdfBitmap(
            Native.FPDFImageObj_GetRenderedBitmap(_document.Handle, _page?.Handle ?? IntPtr.Zero, Handle));
    }

    public byte[] GetImageDataRaw()
    {
        var length = Native.FPDFImageObj_GetImageDataRaw(Handle, null, (IntPtr) 0);
        var buffer = new byte[(int) length];
        Native.FPDFImageObj_GetImageDataRaw(Handle, buffer, length);
        return buffer;
    }

    public byte[] GetImageDataDecoded()
    {
        var length = Native.FPDFImageObj_GetImageDataDecoded(Handle, null, (IntPtr) 0);
        var buffer = new byte[(int) length];
        Native.FPDFImageObj_GetImageDataDecoded(Handle, buffer, length);
        return buffer;
    }

    public PdfImageMetadata ImageMetadata
    {
        get
        {
            var metadata = new PdfImageMetadata();
            Native.FPDFImageObj_GetImageMetadata(Handle, _page?.Handle ?? IntPtr.Zero, ref metadata);
            return metadata;
        }
    }

    public int ImageFilterCount => Native.FPDFImageObj_GetImageFilterCount(Handle);

    public string GetImageFilter(int index)
    {
        var length = Native.FPDFImageObj_GetImageFilter(Handle, index, null, (IntPtr) 0);
        var buffer = new byte[(int) length];
        Native.FPDFImageObj_GetImageFilter(Handle, index, buffer, length);
        return Encoding.UTF8.GetString(buffer, 0, buffer.Length - 1);
    }

    protected override void DisposeHandle()
    {
        if (_owned)
        {
            Native.FPDFPageObj_Destroy(Handle);
        }
    }

    public bool HasFilters(params string[] filters)
    {
        if (filters.Length != ImageFilterCount)
        {
            return false;
        }
        for (int i = 0; i < filters.Length; i++)
        {
            if (!filters[i].Equals(GetImageFilter(i), StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }
        return true;
    }
}