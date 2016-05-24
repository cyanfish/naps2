using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;

namespace NAPS2.Ocr
{
    public class DownloadInfo
    {
        private const string URL_FORMAT = @"https://sourceforge.net/projects/naps2/files/components/tesseract-3.04/{0}/download";

        public DownloadInfo(string fileName, double size, string sha1, DownloadFormat format)
        {
            FileName = fileName;
            Url = string.Format(URL_FORMAT, fileName);
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
