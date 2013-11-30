using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NAPS2.Update
{
    public class UrlTextReader : IUrlTextReader
    {
        private readonly IUrlStreamReader urlStreamReader;

        public UrlTextReader(IUrlStreamReader urlStreamReader)
        {
            this.urlStreamReader = urlStreamReader;
        }

        public string DownloadText(string url)
        {
            using (var streamReader = new StreamReader(urlStreamReader.OpenStream(url)))
            {
                return streamReader.ReadToEnd();
            }
        }
    }
}
