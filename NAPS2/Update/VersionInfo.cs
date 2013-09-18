using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NAPS2.Update
{
    public class VersionInfo
    {
        public Edition Edition { get; set; }

        // TODO: String or Version object?
        public Version LatestVersion { get; set; }

        public string FileName { get; set; }

        public string DownloadUrl { get; set; }
    }
}
