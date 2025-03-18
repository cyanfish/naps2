namespace NAPS2.Update;

public class UpdateInfo
{
    public UpdateInfo(string name, string downloadUrl, byte[] sha256, byte[] signature256)
    {
        Name = name;
        DownloadUrl = downloadUrl;
        Sha256 = sha256;
        Signature256 = signature256;
    }

    public string Name { get; }

    public string DownloadUrl { get; }

    public byte[] Sha256 { get; }

    public byte[] Signature256 { get; }
}