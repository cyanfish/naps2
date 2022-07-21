namespace NAPS2.ImportExport.Pdf.Pdfium;

public record struct PdfMatrix(float a, float b, float c, float d, float e, float f)
{
    public static PdfMatrix FillPage(float width, float height)
    {
        return new PdfMatrix(width, 0, 0, height, 0, 0);
    }
}