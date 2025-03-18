namespace NAPS2.Update;

public class UpdateInfo
{
    public UpdateInfo(string name, string downloadUrl, byte[] sha256, byte[] signature)
    {
        Name = name;
        DownloadUrl = downloadUrl;
        Sha256 = sha256;
        Signature = signature;
    }

    public string Name { get; }

    public string DownloadUrl { get; }

    public byte[] Sha256 { get; }

    public byte[] Signature { get; }
}