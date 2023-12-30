namespace NAPS2.Pdf;

/// <summary>
/// Represents standard PDF metadata (e.g. author, subject, title).
/// </summary>
public record PdfMetadata
{
    public string Author { get; init; } = "";
    public string Creator { get; init; } = "";
    public string Keywords { get; init; } = "";
    public string Subject { get; init; } = "";
    public string Title { get; init; } = "";
}