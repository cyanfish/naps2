using System.Collections.Generic;
using System.Linq;
using NAPS2.Scan;

namespace NAPS2.Config
{
    public class StubProfileManager : IProfileManager
    {
        public List<ScanProfile> Profiles { get; } = new List<ScanProfile>();

        public ScanProfile DefaultProfile
        {
            get => Profiles.FirstOrDefault(x => x.IsDefault) ?? Profiles.FirstOrDefault();
            set
            {
                foreach (var p in Profiles)
                {
                    p.IsDefault = false;
                }
                value.IsDefault = true;
            }
        }

        public void Load()
        {
        }

        public void Save()
        {
        }
    }
}
