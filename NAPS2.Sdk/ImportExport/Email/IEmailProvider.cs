namespace NAPS2.ImportExport.Email;

public interface IEmailProvider
{
    Task<bool> SendEmail(EmailMessage emailMessage, ProgressHandler progress = default);
}