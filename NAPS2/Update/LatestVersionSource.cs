using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NAPS2.Update
{
    public class LatestVersionSource : ILatestVersionSource
    {
        private readonly IUrlTextReader urlTextReader;
        private readonly string versionFileUrl;

        public LatestVersionSource(string versionFileUrl, IUrlTextReader urlTextReader)
        {
            this.versionFileUrl = versionFileUrl;
            this.urlTextReader = urlTextReader;
        }

        public Task<string> GetLatestVersion()
        {
            var task = new Task<string>(() => urlTextReader.DownloadText(versionFileUrl).Trim());
            task.Start();
            return task;
        }
    }
}
