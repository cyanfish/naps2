using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Platform
{
    public class WindowsSystemCompat : ISystemCompat
    {
        public bool IsWiaDriverSupported => true;

        public bool IsTwainDriverSupported => true;

        public bool IsSaneDriverSupported => false;
    }
}
