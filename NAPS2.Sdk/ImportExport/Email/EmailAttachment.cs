namespace NAPS2.ImportExport.Email;

/// <summary>
/// Represents an attachment for an EmailMessage.
/// </summary>
internal record EmailAttachment
{
    /// <summary>
    /// The path of the source file to be attached.
    /// </summary>
    public required string FilePath { get; init; }

    /// <summary>
    /// The name of the attachment (usually the source file name).
    /// </summary>
    public required string AttachmentName { get; init; }
}