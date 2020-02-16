namespace NAPS2.Dependencies
{
    public class DownloadMirror
    {
        private readonly PlatformSupport _platformSupport;
        private readonly string _urlFormat;

        public DownloadMirror(PlatformSupport platformSupport, string urlFormat)
        {
            _platformSupport = platformSupport;
            _urlFormat = urlFormat;
        }

        public bool IsSupported => _platformSupport.Validate();

        public string Url(string subpath)
        {
            return string.Format(_urlFormat, subpath);
        }
    }
}
