namespace NAPS2.Pdf;

/// <summary>
/// Configuration for how images are compressed in exported PDFs.
/// </summary>
public record PdfCompression
{
    /// <summary>
    /// Whether black and white images can be compressed with JBIG2 (lossless). If true, JBIG2 is used when it produces
    /// a smaller result than the alternatives (e.g. CCITT G4).
    /// </summary>
    public bool AllowJbig2 { get; init; } = true;
}
