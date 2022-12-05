namespace NAPS2.ImportExport.Email;

/// <summary>
/// Represents an attachment for an EmailMessage.
/// </summary>
/// <param name="FilePath">The path of the source file to be attached.</param>
/// <param name="AttachmentName">The name of the attachment (usually the source file name).</param>
public record EmailAttachment(string FilePath, string AttachmentName);