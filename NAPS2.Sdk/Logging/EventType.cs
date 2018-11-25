using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Logging
{
    [Flags]
    public enum EventType
    {
        None = 0,
        Scan = 1 << 0,
        SavePdf = 1 << 1,
        SaveImages = 1 << 2,
        Save = SavePdf | SaveImages,
        Email = 1 << 3,
        Print = 1 << 4,
        Update = 1 << 5,
        All = Scan | Save | Email | Print | Update
    }
}