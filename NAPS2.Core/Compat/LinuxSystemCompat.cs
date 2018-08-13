using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Compat
{
    public class LinuxSystemCompat : ISystemCompat
    {
        public bool IsWiaDriverSupported => false;

        public bool IsTwainDriverSupported => false;

        public bool IsSaneDriverSupported => true;
    }
}
