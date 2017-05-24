using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Dependencies
{
    public class DownloadInfo
    {
        public DownloadInfo(string fileName, string urlFormat, double size, string sha1, DownloadFormat format)
        {
            FileName = fileName;
            Url = string.Format(urlFormat, fileName);
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
