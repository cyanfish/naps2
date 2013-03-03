using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Scan;

namespace NAPS2
{
    public interface IProfileManager
    {
        List<ScanSettings> Profiles { get; }
        void Load();
        void Save();
    }
}
