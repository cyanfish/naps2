namespace NAPS2.Pdf;

public record PdfExportParams
{
    public PdfExportParams()
    {
    }

    public PdfExportParams(PdfMetadata metadata, PdfEncryption encryption, PdfCompat compat)
    {
        Metadata = metadata;
        Encryption = encryption;
        Compat = compat;
    }

    public PdfMetadata Metadata { get; init; } = new();

    public PdfEncryption Encryption { get; init; } = new();
    
    public PdfCompat Compat { get; init; } = PdfCompat.Default;
}