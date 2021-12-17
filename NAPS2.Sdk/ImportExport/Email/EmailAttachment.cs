namespace NAPS2.ImportExport.Email;

public class EmailAttachment
{
    public EmailAttachment(string filePath, string attachmentName)
    {
        FilePath = filePath;
        AttachmentName = attachmentName;
    }
        
    /// <summary>
    /// The path of the source file to be attached.
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// The name of the attachment (usually the source file name).
    /// </summary>
    public string AttachmentName { get; }
}