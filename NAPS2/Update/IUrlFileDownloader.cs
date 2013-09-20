using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NAPS2.Update
{
    public interface IUrlFileDownloader
    {
        void DownloadFile(string url, string targetPath);
    }
}
