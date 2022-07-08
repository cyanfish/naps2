namespace NAPS2.Tools.Project.Packaging;

public record PackageFile(string SourceDir, string DestDir, string FileName, string? DestFileName = null)
{
    public string SourcePath => Path.Combine(SourceDir, FileName);

    public string DestPath => Path.Combine(DestDir, DestFileName ?? FileName);
}