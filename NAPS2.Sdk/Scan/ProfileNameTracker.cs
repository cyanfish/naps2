using System;
using System.Collections.Generic;
using System.Linq;
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
        public void RenamingProfile(string oldName, string newName)
        {
            if (string.IsNullOrEmpty(oldName))
            {
                return;
            }
            if (UserConfig.Current.LastBatchSettings != null)
            {
                if (UserConfig.Current.LastBatchSettings.ProfileDisplayName == oldName)
                {
                    UserConfig.Current.LastBatchSettings.ProfileDisplayName = newName;
                    UserConfig.Manager.Save();
                }
            }
        }

        public void DeletingProfile(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return;
            }
            if (UserConfig.Current.LastBatchSettings != null)
            {
                if (UserConfig.Current.LastBatchSettings.ProfileDisplayName == name)
                {
                    UserConfig.Current.LastBatchSettings.ProfileDisplayName = null;
                    UserConfig.Manager.Save();
                }
            }
        }
    }
}
