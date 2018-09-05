using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace NAPS2.Scan.Wia
{
    public interface IWiaTransfer
    {
        Stream Transfer(int pageNumber, WiaBackgroundEventLoop eventLoop, IWin32Window dialogParent, string format);
    }
}
