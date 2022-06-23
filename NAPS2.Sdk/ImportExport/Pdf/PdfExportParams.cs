namespace NAPS2.ImportExport.Pdf;

public record PdfExportParams(PdfMetadata? Metadata = null, PdfEncryption? Encryption = null, PdfCompat Compat = PdfCompat.Default);