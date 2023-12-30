namespace NAPS2.Pdf;

/// <summary>
/// Configuration for PDF encryption (e.g. passwords, permissions).
/// </summary>
public record PdfEncryption
{
    public bool EncryptPdf { get; init; }
    public string? UserPassword { get; init; }
    public string? OwnerPassword { get; init; }
    public bool AllowContentCopyingForAccessibility { get; init; } = true;
    public bool AllowAnnotations { get; init; } = true;
    public bool AllowDocumentAssembly { get; init; } = true;
    public bool AllowContentCopying { get; init; } = true;
    public bool AllowFormFilling { get; init; } = true;
    public bool AllowFullQualityPrinting { get; init; } = true;
    public bool AllowDocumentModification { get; init; } = true;
    public bool AllowPrinting { get; init; } = true;
}