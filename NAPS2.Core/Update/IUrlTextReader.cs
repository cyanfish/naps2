using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Update
{
    public interface IUrlTextReader
    {
        string DownloadText(string url);
    }
}
