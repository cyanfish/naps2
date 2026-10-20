using NAPS2.ImportExport;

namespace NAPS2.Pdf;

internal static class PdfMetadataExtensions
{
    public static PdfMetadata Substitute(this PdfMetadata metadata, Placeholders placeholders,
        string? barcodeValue = null)
    {
        return metadata with
        {
            Author = placeholders.Substitute(metadata.Author, false, barcodeValue: barcodeValue),
            Creator = placeholders.Substitute(metadata.Creator, false, barcodeValue: barcodeValue),
            Keywords = placeholders.Substitute(metadata.Keywords, false, barcodeValue: barcodeValue),
            Subject = placeholders.Substitute(metadata.Subject, false, barcodeValue: barcodeValue),
            Title = placeholders.Substitute(metadata.Title, false, barcodeValue: barcodeValue)
        };
    }
}
