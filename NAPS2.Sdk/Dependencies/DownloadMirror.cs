namespace NAPS2.Dependencies
{
    public class DownloadMirror
    {
        private readonly PlatformSupport platformSupport;
        private readonly string urlFormat;

        public DownloadMirror(PlatformSupport platformSupport, string urlFormat)
        {
            this.platformSupport = platformSupport;
            this.urlFormat = urlFormat;
        }

        public bool IsSupported => platformSupport.Validate();

        public string Url(string subpath)
        {
            return string.Format(urlFormat, subpath);
        }
    }
}
