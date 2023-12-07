using System.Collections.Immutable;
using NAPS2.Scan;

namespace NAPS2.Config;

public class StubProfileManager : IProfileManager
{
    private readonly List<ScanProfile> _profiles = [];

    public ImmutableList<ScanProfile> Profiles => ImmutableList.CreateRange(_profiles);

    public void Mutate(ListMutation<ScanProfile> mutation, ISelectable<ScanProfile> selectable)
    {
        mutation.Apply(_profiles, selectable);
        Save();
    }

    public void Mutate(ListMutation<ScanProfile> mutation, ListSelection<ScanProfile> selection)
    {
        mutation.Apply(_profiles, ref selection);
        Save();
    }

    public ScanProfile? DefaultProfile
    {
        get => Profiles.FirstOrDefault(x => x.IsDefault) ?? Profiles.FirstOrDefault();
        set
        {
            foreach (var p in Profiles)
            {
                p.IsDefault = false;
            }
            if (value != null)
            {
                value.IsDefault = true;
            }
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