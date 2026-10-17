using NAPS2.ImportExport;

namespace NAPS2.Pdf;

internal static class PdfMetadataExtensions
{
    public static PdfMetadata Substitute(this PdfMetadata metadata, Placeholders placeholders)
    {
        return metadata with
        {
            Author = placeholders.Substitute(metadata.Author, false),
            Creator = placeholders.Substitute(metadata.Creator, false),
            Keywords = placeholders.Substitute(metadata.Keywords, false),
            Subject = placeholders.Substitute(metadata.Subject, false),
            Title = placeholders.Substitute(metadata.Title, false)
        };
    }
}
