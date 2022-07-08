namespace NAPS2.Tools.Project.Packaging;

public class PackageInfo
{
    public PackageInfo(Platform platform, string version)
    {
        Platform = platform;
        Version = version;
    }

    public Platform Platform { get; }
    
    public string Version { get; }

    public string FileName => $"naps2-{Version}-{Platform.PackageName()}";

    public List<PackageFile> Files { get; } = new();

    public void AddFile(FileInfo file, string destFolder, string? destFileName = null)
    {
        if (file.DirectoryName == null)
        {
            throw new ArgumentException();
        }
        Files.Add(new PackageFile(file.DirectoryName, destFolder, file.Name, destFileName));
    }
}