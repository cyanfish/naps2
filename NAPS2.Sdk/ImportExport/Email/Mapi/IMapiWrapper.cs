namespace NAPS2.ImportExport.Email.Mapi;

internal interface IMapiWrapper
{
    bool CanLoadClient(string? clientName);

    Task<MapiSendMailReturnCode> SendEmail(string? clientName, EmailMessage message);
}