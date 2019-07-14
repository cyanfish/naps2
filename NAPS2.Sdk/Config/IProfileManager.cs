using System.Collections.Generic;
using NAPS2.Scan;

namespace NAPS2.Config
{
    public interface IProfileManager
    {
        List<ScanProfile> Profiles { get; }
        ScanProfile DefaultProfile { get; set; }
        void Load();
        void Save();
    }
}
