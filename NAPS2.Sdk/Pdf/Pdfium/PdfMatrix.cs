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

    /// <summary>
    /// Returns the matrix that applies this transform followed by the other transform (multiplying this × other).
    /// </summary>
    public PdfMatrix Then(PdfMatrix other)
    {
        return new PdfMatrix(
            a * other.a + b * other.c,
            a * other.b + b * other.d,
            c * other.a + d * other.c,
            c * other.b + d * other.d,
            e * other.a + f * other.c + other.e,
            e * other.b + f * other.d + other.f);
    }

    public static PdfMatrix FillPage(float width, float height)
    {
        return new PdfMatrix(width, 0, 0, height, 0, 0);
    }

    public static PdfMatrix FillPage(PdfRect box)
    {
        return new PdfMatrix(box.Right - box.Left, 0, 0, box.Top - box.Bottom, box.Left, box.Bottom);
    }
}