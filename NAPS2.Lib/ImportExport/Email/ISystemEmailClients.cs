namespace NAPS2.ImportExport.Email;

public interface ISystemEmailClients
{
    string[] GetNames();
    string? GetDefaultName();
    IMemoryImage? LoadIcon(string clientName);
}