using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Platform
{
    public class LinuxSystemCompat : ISystemCompat
    {
        public bool IsWiaDriverSupported => false;

        public bool IsWia20Supported => false;

        public bool IsTwainDriverSupported => false;

        public bool IsSaneDriverSupported => true;

        public bool CanUseWin32 => false;

        public bool UseUnixFontResolver => true;
    }
}
