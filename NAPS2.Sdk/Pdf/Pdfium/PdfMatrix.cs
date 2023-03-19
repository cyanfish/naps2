namespace NAPS2.Pdf.Pdfium;

// See PDF standard 1.7 section 4.2.2 and 4.2.3
internal record struct PdfMatrix(float a, float b, float c, float d, float e, float f)
{
    private const float TOLERANCE = 0.00001f;

    public static bool EqualsWithinTolerance(PdfMatrix first, PdfMatrix second)
    {
        return Math.Abs(first.a - second.a) < TOLERANCE &&
               Math.Abs(first.b - second.b) < TOLERANCE &&
               Math.Abs(first.c - second.c) < TOLERANCE &&
               Math.Abs(first.d - second.d) < TOLERANCE &&
               Math.Abs(first.e - second.e) < TOLERANCE &&
               Math.Abs(first.f - second.f) < TOLERANCE;
    }

    public static PdfMatrix FillPage(float width, float height)
    {
        return new PdfMatrix(width, 0, 0, height, 0, 0);
    }
}