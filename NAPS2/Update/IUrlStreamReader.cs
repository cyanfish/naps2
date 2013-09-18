using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NAPS2.Update
{
    public interface IUrlStreamReader
    {
        Stream OpenStream(string url);
    }
}
