namespace NAPS2.ImportExport.Email.Mapi
{
    public interface IMapiWrapper
    {
        bool CanLoadClient { get; }

        MapiSendMailReturnCode SendEmail(EmailMessage message);
    }
}