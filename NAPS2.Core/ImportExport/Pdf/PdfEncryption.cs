namespace NAPS2.ImportExport.Pdf
{
    public class PdfEncryption
    {
        public bool EncryptPdf { get; set; }
        public string UserPassword { get; set; }
        public string OwnerPassword { get; set; }
        public bool AllowContentCopyingForAccessibility { get; set; }
        public bool AllowAnnotations { get; set; }
        public bool AllowDocumentAssembly { get; set; }
        public bool AllowContentCopying { get; set; }
        public bool AllowFormFilling { get; set; }
        public bool AllowFullQualityPrinting { get; set; }
        public bool AllowDocumentModification { get; set; }
        public bool AllowPrinting { get; set; }
    }
}