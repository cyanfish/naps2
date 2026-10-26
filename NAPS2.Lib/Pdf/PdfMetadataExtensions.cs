using NAPS2.ImportExport;

namespace NAPS2.Pdf;

internal static class PdfMetadataExtensions
{
    public static PdfMetadata Substitute(this PdfMetadata metadata, NameManager nameManager,
        DateTime? dateTime = null)
    {
        var context = new NameContext { DateTime = dateTime ?? DateTime.Now, IsFilePath = false };
        return metadata with
        {
            Author = nameManager.Get(metadata.Author, context),
            Creator = nameManager.Get(metadata.Creator, context),
            Keywords = nameManager.Get(metadata.Keywords, context),
            Subject = nameManager.Get(metadata.Subject, context),
            Title = nameManager.Get(metadata.Title, context)
        };
    }
}
