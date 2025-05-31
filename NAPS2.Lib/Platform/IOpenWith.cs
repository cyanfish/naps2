namespace NAPS2.Platform;

public interface IOpenWith
{
    public IEnumerable<OpenWithEntry> GetEntries(string fileExt);
    public void OpenWith(string entryId, IEnumerable<string> filePaths);
    public IMemoryImage? LoadIcon(OpenWithEntry entry);
}

public record OpenWithEntry(string Id, string Name, string IconPath, int IconIndex);