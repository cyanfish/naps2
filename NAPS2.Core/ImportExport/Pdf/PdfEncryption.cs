namespace NAPS2.ImportExport.Pdf
{
    public class PdfEncryption
    {
        public PdfEncryption()
        {
        }

        public bool EncryptPdf { get; set; }
        public string UserPassword { get; set; }
        public string OwnerPassword { get; set; }
        public bool PermitAccessibilityExtractContent { get; set; }
        public bool PermitAnnotations { get; set; }
        public bool PermitAssembleDocument { get; set; }
        public bool PermitExtractContent { get; set; }
        public bool PermitFormsFill { get; set; }
        public bool PermitFullQualityPrint { get; set; }
        public bool PermitModifyDocument { get; set; }
        public bool PermitPrint { get; set; }
    }
}