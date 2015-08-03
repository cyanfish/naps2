using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NAPS2.Scan.Wia
{
    public interface IWiaTransfer
    {
        Stream Transfer(int pageNumber, WiaBackgroundEventLoop eventLoop, string format);
    }
}
