using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Dependencies
{
    public class DownloadInfo
    {
        public DownloadInfo(string fileName, string urlFormat, string xpUrlFormat, double size, string sha1, DownloadFormat format)
        {
            FileName = fileName;
            if (PlatformSupport.WindowsXp.Validate())
            {
                Url = string.Format(xpUrlFormat, fileName);
            }
            else
            {
                Url = string.Format(urlFormat, fileName);
            }
            Size = size;
            Sha1 = sha1;
            Format = format;
        }

        public string FileName { get; }

        public string Url { get; }

        public DownloadFormat Format { get; }

        public double Size { get; }

        public string Sha1 { get; }
    }
}
