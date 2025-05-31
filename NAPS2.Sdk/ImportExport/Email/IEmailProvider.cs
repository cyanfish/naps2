namespace NAPS2.ImportExport.Email;

internal interface IEmailProvider
{
    Task<bool> SendEmail(EmailMessage emailMessage, ProgressHandler progress = default);
    bool ShowInList { get; }
    bool CanSelectInList { get; }
}