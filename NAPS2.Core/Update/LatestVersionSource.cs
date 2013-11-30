using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NAPS2.Update
{
    public class LatestVersionSource : ILatestVersionSource
    {
        private static readonly Lazy<XmlSerializer> VersionInfoSerializer =
            new Lazy<XmlSerializer>(() => new XmlSerializer(typeof(List<VersionInfo>)));

        private readonly IUrlStreamReader urlStreamReader;
        private readonly string versionFileUrl;

        public LatestVersionSource(string versionFileUrl, IUrlStreamReader urlStreamReader)
        {
            this.versionFileUrl = versionFileUrl;
            this.urlStreamReader = urlStreamReader;
        }

        public Task<List<VersionInfo>> GetLatestVersionInfo()
        {
            return Task.Factory.StartNew(() =>
            {
                var stream = urlStreamReader.OpenStream(versionFileUrl);
                var versionInfos = (List<VersionInfo>)VersionInfoSerializer.Value.Deserialize(stream);
                // Do some post-processing by replacing string arguments ("{0}", "{1}") with the appropriate content
                // This is supported so that maintaining version.xml is easier,
                // i.e. the version number needs to be changed in fewer places
                foreach (var versionInfo in versionInfos)
                {
                    versionInfo.FileName = string.Format(versionInfo.FileName, versionInfo.LatestVersion);
                    versionInfo.DownloadUrl = string.Format(versionInfo.DownloadUrl,
                        versionInfo.LatestVersion, versionInfo.FileName);
                }
                return versionInfos;
            });
        }
    }
}
