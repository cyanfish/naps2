using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAPS2.Config;

namespace NAPS2.Scan
{
    public class ProfileNameTracker
    {
        private readonly IUserConfigManager userConfigManager;

        public ProfileNameTracker(IUserConfigManager userConfigManager)
        {
            this.userConfigManager = userConfigManager;
        }

        public void RenamingProfile(string oldName, string newName)
        {
            if (string.IsNullOrEmpty(oldName))
            {
                return;
            }
            if (userConfigManager.Config.LastBatchSettings != null)
            {
                if (userConfigManager.Config.LastBatchSettings.ProfileDisplayName == oldName)
                {
                    userConfigManager.Config.LastBatchSettings.ProfileDisplayName = newName;
                    userConfigManager.Save();
                }
            }
        }

        public void DeletingProfile(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return;
            }
            if (userConfigManager.Config.LastBatchSettings != null)
            {
                if (userConfigManager.Config.LastBatchSettings.ProfileDisplayName == name)
                {
                    userConfigManager.Config.LastBatchSettings.ProfileDisplayName = null;
                    userConfigManager.Save();
                }
            }
        }
    }
}
