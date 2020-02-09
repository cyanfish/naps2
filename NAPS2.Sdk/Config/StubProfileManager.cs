using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using NAPS2.Images;
using NAPS2.Scan;

namespace NAPS2.Config
{
    public class StubProfileManager : IProfileManager
    {
        private readonly List<ScanProfile> profiles = new List<ScanProfile>();

        public ImmutableList<ScanProfile> Profiles => ImmutableList.CreateRange(profiles);

        public void Mutate(ListMutation<ScanProfile> mutation, ISelectable<ScanProfile> selectable)
        {
            mutation.Apply(profiles, selectable);
            Save();
        }

        public void Mutate(ListMutation<ScanProfile> mutation, ListSelection<ScanProfile> selection)
        {
            mutation.Apply(profiles, ref selection);
            Save();
        }

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
                Save();
            }
        }

        public void Load()
        {
        }

        public void Save()
        {
            ProfilesUpdated?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler? ProfilesUpdated;
    }
}
