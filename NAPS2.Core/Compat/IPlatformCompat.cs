using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NAPS2.Compat
{
    public interface IPlatformCompat
    {
        bool UseToolStripRenderHack { get; }
    }
}
