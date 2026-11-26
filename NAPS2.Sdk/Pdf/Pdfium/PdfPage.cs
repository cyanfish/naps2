namespace NAPS2.Pdf.Pdfium;

internal class PdfPage : NativePdfiumObject
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

    public PdfRect BoundingBox
    {
        get
        {
            if (!Native.FPDF_GetPageBoundingBox(Handle, out var rect))
            {
                throw new Exception("Could not get page bounding box");
            }
            return rect;
        }
    }

    // Pdfium returns 0-3 mapping to 0, 90, 180, 270
    public int Rotation => Native.FPDFPage_GetRotation(Handle) * 90;

    public PdfMatrix DisplayToUserSpaceMatrix
    {
        get
        {
            var box = BoundingBox;
            return Rotation switch
            {
                90 => new PdfMatrix(0, 1, -1, 0, box.Right, box.Bottom),
                180 => new PdfMatrix(-1, 0, 0, -1, box.Right, box.Top),
                270 => new PdfMatrix(0, -1, 1, 0, box.Left, box.Top),
                _ => new PdfMatrix(1, 0, 0, 1, box.Left, box.Bottom)
            };
        }
    }

    public (int x, int y) PageToDevice(int width, int height, double x, double y)
    {
        if (!Native.FPDF_PageToDevice(Handle, 0, 0, width, height, 0, x, y, out var deviceX, out var deviceY))
        {
            throw new Exception("Could not map page coordinates to device coordinates");
        }
        return (deviceX, deviceY);
    }

    public bool HasTransparency => Native.FPDFPage_HasTransparency(Handle);

    public PdfText GetText()
    {
        return new PdfText(Native.FPDFText_LoadPage(Handle));
    }

    public int AnnotCount => Native.FPDFPage_GetAnnotCount(Handle);

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