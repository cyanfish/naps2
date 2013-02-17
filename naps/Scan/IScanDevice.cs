using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NAPS
{
    public interface IScanDevice
    {
        string ID { get; }
        string Name { get; }
    }
}
