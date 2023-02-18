using System.Runtime.InteropServices;

namespace NAPS2.Pdf.Pdfium;

internal struct PdfImageMetadata
{
    public int Width;
    public int Height;
    public float HorizontalDpi;
    public float VerticalDpi;
    public int BitsPerPixel;
    [MarshalAs(UnmanagedType.I4)]
    public Colorspace Colorspace;
    public int MarkedContentId;
}