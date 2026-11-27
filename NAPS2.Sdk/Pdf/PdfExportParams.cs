namespace NAPS2.Pdf;

/// <summary>
/// Additional parameters for exporting PDFs (metadata, encryption, compatibility, compression).
/// </summary>
public record PdfExportParams
{
    public PdfExportParams()
    {
    }

    public PdfExportParams(PdfMetadata metadata, PdfEncryption encryption, PdfCompat compat,
        PdfCompression? compression = null)
    {
        Metadata = metadata;
        Encryption = encryption;
        Compat = compat;
        Compression = compression ?? new PdfCompression();
    }

    public PdfMetadata Metadata { get; init; } = new();

    public PdfEncryption Encryption { get; init; } = new();
    
    public PdfCompat Compat { get; init; } = PdfCompat.Default;

    public PdfCompression Compression { get; init; } = new();
}