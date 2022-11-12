namespace NAPS2.Dependencies;

public class DownloadMirror
{
    private readonly string _urlFormat;

    public DownloadMirror(string urlFormat)
    {
        _urlFormat = urlFormat;
    }

    public string Url(string subpath)
    {
        return string.Format(_urlFormat, subpath);
    }
}