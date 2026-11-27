using NAPS2.ImportExport;

namespace NAPS2.Pdf;

internal static class PdfMetadataExtensions
{
    public static PdfMetadata Substitute(this PdfMetadata metadata, NameManager nameManager,
        DateTime? dateTime = null)
    {
        return Substitute(metadata, nameManager, new NameContext
        {
            DateTime = dateTime ?? DateTime.Now,
            IsFilePath = false
        });
    }

    public static PdfMetadata Substitute(this PdfMetadata metadata, NameManager nameManager,
        NameContext context)
    {
        context = context with
        {
            IsFilePath = false,
            Numbering = Numbering.Placeholder,
            BatchIndex = null,
            BatchCount = null
        };
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
