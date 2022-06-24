namespace NAPS2.ImportExport.Pdf;

public record PdfMetadata
{
    public string Author { get; init; } = "";
    public string Creator { get; init; } = "";
    public string Keywords { get; init; } = "";
    public string Subject { get; init; } = "";
    public string Title { get; init; } = "";
}