using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Platform
{
    public interface ISystemCompat
    {
        bool IsWiaDriverSupported { get; }

        bool IsTwainDriverSupported { get; }

        bool IsSaneDriverSupported { get; }

        bool CanUseWin32 { get; }

        bool UseUnixFontResolver { get; }

        bool IsWia20Supported { get; }
    }
}
