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
        private readonly ConfigProvider<CommonConfig> configProvider;
        private readonly ConfigScopes configScopes;

        public ProfileNameTracker(ConfigProvider<CommonConfig> configProvider, ConfigScopes configScopes)
        {
            this.configProvider = configProvider;
            this.configScopes = configScopes;
        }

        public void RenamingProfile(string oldName, string newName)
        {
            if (string.IsNullOrEmpty(oldName))
            {
                return;
            }
            if (configProvider.Get(c => c.BatchSettings.ProfileDisplayName) == oldName)
            {
                configScopes.User.Set(c => c.BatchSettings.ProfileDisplayName = newName);
            }
        }

        public void DeletingProfile(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return;
            }
            if (configProvider.Get(c => c.BatchSettings.ProfileDisplayName) == name)
            {
                configScopes.User.Set(c => c.BatchSettings.ProfileDisplayName = "");
            }
        }
    }
}
