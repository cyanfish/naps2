using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NAPS2.Update
{
    public class UrlFileDownloader : IUrlFileDownloader
    {
        private readonly IUrlStreamReader urlStreamReader;

        public UrlFileDownloader(IUrlStreamReader urlStreamReader)
        {
            this.urlStreamReader = urlStreamReader;
        }

        public void DownloadFile(string url, string targetPath)
        {
            using (Stream sourceStream = urlStreamReader.OpenStream(url))
            using (Stream targetStream = new FileStream(targetPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None))
            {
                sourceStream.CopyTo(targetStream);
            }
        }
    }
}