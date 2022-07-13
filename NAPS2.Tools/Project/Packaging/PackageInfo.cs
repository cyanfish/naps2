namespace NAPS2.Tools.Project.Packaging;

public class PackageInfo
{
    private readonly List<PackageFile> _files = new();
    private readonly HashSet<string> _destPaths = new();

    public PackageInfo(Platform platform, string version)
    {
        Platform = platform;
        Version = version;
    }

    public Platform Platform { get; }
    
    public string Version { get; }

    public string GetPath(string ext)
    {
        return ProjectHelper.GetPackagePath(ext, Platform, Version);
    }

    public IEnumerable<PackageFile> Files => _files;

    public HashSet<string> Languages { get; } = new();

    public void AddFile(FileInfo file, string destFolder, string? destFileName = null)
    {
        if (file.DirectoryName == null)
        {
            throw new ArgumentException();
        }
        AddFile(new PackageFile(file.DirectoryName, destFolder, file.Name, destFileName));
    }

    public void AddFile(PackageFile file)
    {
        if (_destPaths.Add(file.DestPath))
        {
            _files.Add(file);
        }
    }
}