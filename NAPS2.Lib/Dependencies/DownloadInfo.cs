namespace NAPS2.Dependencies;

public class DownloadInfo
{
    public DownloadInfo(string fileName, List<DownloadMirror> mirrors, double size, string sha256, DownloadFormat format)
    {
        FileName = fileName;
        Urls = mirrors.Select(x => x.Url(fileName)).ToList();
        Size = size;
        Sha256 = sha256;
        Format = format;
    }

    public string FileName { get; }

    public List<string> Urls { get; }

    public DownloadFormat Format { get; }

    public double Size { get; }

    public string Sha256 { get; }
}