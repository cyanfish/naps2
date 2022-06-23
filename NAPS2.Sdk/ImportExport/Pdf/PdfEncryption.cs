namespace NAPS2.ImportExport.Pdf;

public class PdfEncryption
{
    public bool EncryptPdf { get; set; }
    public string? UserPassword { get; set; }
    public string? OwnerPassword { get; set; }
    public bool AllowContentCopyingForAccessibility { get; set; } = true;
    public bool AllowAnnotations { get; set; } = true;
    public bool AllowDocumentAssembly { get; set; } = true;
    public bool AllowContentCopying { get; set; } = true;
    public bool AllowFormFilling { get; set; } = true;
    public bool AllowFullQualityPrinting { get; set; } = true;
    public bool AllowDocumentModification { get; set; } = true;
    public bool AllowPrinting { get; set; } = true;
}