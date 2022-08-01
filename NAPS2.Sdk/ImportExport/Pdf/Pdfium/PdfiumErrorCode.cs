namespace NAPS2.ImportExport.Pdf.Pdfium;

public enum PdfiumErrorCode
{
    Success = 0,
    Unknown = 1,
    FileNotFoundOrUnavailable = 2,
    InvalidFileFormat = 3,
    PasswordNeeded = 4,
    UnsupportedSecurity = 5,
    ContentError = 6,
    XfaLoadError = 7,
    XfaLayoutError = 8
}