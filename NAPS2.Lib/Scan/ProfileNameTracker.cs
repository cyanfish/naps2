namespace NAPS2.Scan;

/// <summary>
/// A class used to help keep profile names consistent across forms.
///
/// TODO: This should probably be replaced by an event handler system.
/// </summary>
public class ProfileNameTracker
{
    private readonly Naps2Config _config;

    public ProfileNameTracker(Naps2Config config)
    {
        _config = config;
    }

    public void RenamingProfile(string oldName, string newName)
    {
        if (string.IsNullOrEmpty(oldName))
        {
            return;
        }
        if (_config.Get(c => c.BatchSettings.ProfileDisplayName) == oldName)
        {
            _config.User.Set(c => c.BatchSettings.ProfileDisplayName, newName);
        }
    }

    public void DeletingProfile(string? name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return;
        }
        if (_config.Get(c => c.BatchSettings.ProfileDisplayName) == name)
        {
            _config.User.Set(c => c.BatchSettings.ProfileDisplayName,  "");
        }
    }
}