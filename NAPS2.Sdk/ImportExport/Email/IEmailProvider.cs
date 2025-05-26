namespace NAPS2.ImportExport.Email;

internal interface IEmailProvider
{
    Task<bool> SendEmail(EmailMessage emailMessage, ProgressHandler progress = default);
    bool IsAvailable { get; }
}