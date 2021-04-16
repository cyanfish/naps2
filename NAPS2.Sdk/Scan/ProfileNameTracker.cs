using NAPS2.Config;

namespace NAPS2.Scan
{
    /// <summary>
    /// A class used to help keep profile names consistent across forms.
    ///
    /// TODO: This should probably be replaced by an event handler system.
    /// </summary>
    public class ProfileNameTracker
    {
        private readonly ConfigProvider<CommonConfig> _configProvider;
        private readonly ConfigScopes _configScopes;

        public ProfileNameTracker(ConfigProvider<CommonConfig> configProvider, ConfigScopes configScopes)
        {
            _configProvider = configProvider;
            _configScopes = configScopes;
        }

        public void RenamingProfile(string oldName, string newName)
        {
            if (string.IsNullOrEmpty(oldName))
            {
                return;
            }
            if (_configProvider.Get(c => c.BatchSettings.ProfileDisplayName) == oldName)
            {
                _configScopes.User.Set(c => c.BatchSettings.ProfileDisplayName = newName);
            }
        }

        public void DeletingProfile(string? name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return;
            }
            if (_configProvider.Get(c => c.BatchSettings.ProfileDisplayName) == name)
            {
                _configScopes.User.Set(c => c.BatchSettings.ProfileDisplayName = "");
            }
        }
    }
}
