namespace NAPS2.Platform;

public interface IOpenWith
{
    public IEnumerable<OpenWithEntry> GetEntries(string fileExt);
    public void OpenWith(string entryId, string filePath);
    public IMemoryImage? LoadIcon(OpenWithEntry entry);
}

public record OpenWithEntry(string Id, string Name, string IconPath, int IconIndex);