namespace NAPS2.ImportExport.Email
{
    public interface IEmailer
    {
        bool SendEmail(EmailMessage emailMessage);
    }
}