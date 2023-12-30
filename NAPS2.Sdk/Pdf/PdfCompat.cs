namespace NAPS2.Pdf;

/// <summary>
/// Compatibility format for generating PDFs, e.g. PDF/A (https://en.wikipedia.org/wiki/PDF/A).
/// </summary>
public enum PdfCompat
{
    Default,
    PdfA1B,
    PdfA2B,
    PdfA3B,
    PdfA3U
}