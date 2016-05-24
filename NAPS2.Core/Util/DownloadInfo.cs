using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Util
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

        public string FileName { get; private set; }

        public string Url { get; private set; }

        public DownloadFormat Format { get; private set; }

        public double Size { get; private set; }

        public string Sha1 { get; private set; }
    }
}
