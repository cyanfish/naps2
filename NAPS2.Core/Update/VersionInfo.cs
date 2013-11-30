using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Update
{
    public class VersionInfo
    {
        public Edition Edition { get; set; }

        public string LatestVersion { get; set; }

        public string FileName { get; set; }

        public string DownloadUrl { get; set; }

        public string InstallArguments { get; set; }
    }
}
