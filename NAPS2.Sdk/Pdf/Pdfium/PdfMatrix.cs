namespace NAPS2.Pdf.Pdfium;

// See PDF standard 1.7 section 4.2.2 and 4.2.3
internal record struct PdfMatrix(float a, float b, float c, float d, float e, float f)
{
    public static PdfMatrix FillPage(float width, float height)
    {
        return new PdfMatrix(width, 0, 0, height, 0, 0);
    }
}