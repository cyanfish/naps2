namespace NAPS2.Dependencies;

public interface IExternalComponent
{
    DownloadInfo DownloadInfo { get; }

    string Id { get; }

    bool IsInstalled { get; }

    void Install(string sourcePath);
}