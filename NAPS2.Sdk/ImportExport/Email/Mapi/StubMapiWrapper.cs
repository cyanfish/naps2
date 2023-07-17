namespace NAPS2.ImportExport.Email.Mapi;

internal class StubMapiWrapper : IMapiWrapper
{
    public bool CanLoadClient(string? clientName) => throw new NotSupportedException();

    public Task<MapiSendMailReturnCode> SendEmail(string? clientName, EmailMessage message) =>
        throw new NotSupportedException();
}