namespace NAPS2.ImportExport.Email.Mapi
{
    public interface IMapiWrapper
    {
        MapiSendMailReturnCode SendEmail(EmailMessage message);
    }
}