namespace NAPS2.Pdf;

/// <summary>
/// Additional parameters for exporting PDFs (metadata, encryption, compatibility).
/// </summary>
public record PdfExportParams
{
    public PdfExportParams()
    {
    }

    public PdfExportParams(PdfMetadata metadata, PdfEncryption encryption, PdfCompat compat, int jpegQuality = 75, int resolutionScale = 100)
    {
        Metadata = metadata;
        Encryption = encryption;
        Compat = compat;
        JpegQuality = jpegQuality;
        ResolutionScale = resolutionScale;
    }

    public PdfMetadata Metadata { get; init; } = new();

    public PdfEncryption Encryption { get; init; } = new();
    
    public PdfCompat Compat { get; init; } = PdfCompat.Default;

    public int JpegQuality { get; init; } = 75;

    public int ResolutionScale { get; init; } = 100;
}