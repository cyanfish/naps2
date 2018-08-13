using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NAPS2.Compat
{
    public interface ISystemCompat
    {
        bool IsWiaDriverSupported { get; }

        bool IsTwainDriverSupported { get; }

        bool IsSaneDriverSupported { get; }
    }
}
