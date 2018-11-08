using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Scan.Wia.Native
{
    public interface IWiaProps
    {
        WiaPropertyCollection Properties { get; }
    }
}