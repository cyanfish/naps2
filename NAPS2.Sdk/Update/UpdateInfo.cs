namespace NAPS2.Update
{
    public class UpdateInfo
    {
        public UpdateInfo(string name, string downloadUrl, byte[] sha1, byte[] signature)
        {
            Name = name;
            DownloadUrl = downloadUrl;
            Sha1 = sha1;
            Signature = signature;
        }

        public string Name { get; }

        public string DownloadUrl { get; }

        public byte[] Sha1 { get; }

        public byte[] Signature { get; }
    }
}
