namespace NAPS2.ImportExport.Email;

public class StubSystemEmailClients : ISystemEmailClients
{
    public string[] GetNames() => [];
    public string? GetDefaultName() => null;
    public IMemoryImage? LoadIcon(string clientName) => null;
}