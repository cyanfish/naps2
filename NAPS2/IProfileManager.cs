using NAPS2.Scan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NAPS2
{
    public interface IProfileManager
    {
        void Load();
        List<ScanSettings> Profiles { get; }
        void Save();
    }
}
