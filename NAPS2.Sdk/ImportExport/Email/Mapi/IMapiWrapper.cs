namespace NAPS2.ImportExport.Email.Mapi;

public interface IMapiWrapper
{
    bool CanLoadClient(string clientName);

    Task<MapiSendMailReturnCode> SendEmail(string clientName, EmailMessage message);
}