using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Dependencies
{
    public class DownloadInfo
    {
        public DownloadInfo(string fileName, List<DownloadMirror> mirrors, double size, string sha1, DownloadFormat format)
        {
            FileName = fileName;
            Urls = mirrors.Where(x => x.IsSupported).Select(x => x.Url(fileName)).ToList();
            Size = size;
            Sha1 = sha1;
            Format = format;
        }

        public string FileName { get; }

        public List<string> Urls { get; }

        public DownloadFormat Format { get; }

        public double Size { get; }

        public string Sha1 { get; }
    }
}
