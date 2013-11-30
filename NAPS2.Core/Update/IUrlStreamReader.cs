using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NAPS2.Update
{
    public interface IUrlStreamReader
    {
        Stream OpenStream(string url);
    }
}
