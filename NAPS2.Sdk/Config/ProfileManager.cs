using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NAPS2.Scan;

namespace NAPS2.Config
{
    public class ProfileManager : ConfigManager<List<ScanProfile>>, IProfileManager
    {
        private static IProfileManager _current = new StubProfileManager();

        public static IProfileManager Current
        {
            get => _current;
            set => _current = value ?? throw new ArgumentNullException(nameof(value));
        }

        public ProfileManager(string indexFileName, string primaryFolder, string secondaryFolder)
            : base(indexFileName, primaryFolder, secondaryFolder, () => new List<ScanProfile>(), new ProfileSerializer())
        {
        }

        public List<ScanProfile> Profiles => Config;

        public ScanProfile DefaultProfile
        {
            get
            {
                if (Profiles.Count == 1)
                {
                    return Profiles.First();
                }
                return Profiles.FirstOrDefault(x => x.IsDefault);
            }
            set
            {
                foreach (ScanProfile profile in Profiles)
                {
                    profile.IsDefault = false;
                }
                value.IsDefault = true;
            }
        }

        public override void Load()
        {
            base.Load();
            var upgradedFrom = Config.Select(x => x.UpgradedFrom).FirstOrDefault();
            if (upgradedFrom != null)
            {
                // We've upgraded to a new profiles version, so make a backup in case the user downgrades.
                File.Copy(primaryConfigPath, $"{primaryConfigPath}.v{upgradedFrom}.bak", true);
            }
            if (AppConfig.Current.LockSystemProfiles)
            {
                var systemProfiles = TryLoadConfig(secondaryConfigPath);
                if (systemProfiles != null)
                {
                    foreach (var systemProfile in systemProfiles)
                    {
                        systemProfile.IsLocked = true;
                        systemProfile.IsDeviceLocked = (systemProfile.Device != null || AppConfig.Current.LockUnspecifiedDevices);
                    }
                    var systemProfileNames = new HashSet<string>(systemProfiles.Select(x => x.DisplayName));
                    foreach (var profile in Config.ToList())
                    {
                        if (systemProfileNames.Contains(profile.DisplayName))
                        {
                            // Merge some properties from the user's copy of the profile
                            var systemProfile = systemProfiles.First(x => x.DisplayName == profile.DisplayName);
                            if (systemProfile.Device == null)
                            {
                                systemProfile.Device = profile.Device;
                            }
                            systemProfile.IsDefault = profile.IsDefault;

                            // Delete the user's copy of the profile
                            Config.Remove(profile);
                            // Avoid removing duplicates
                            systemProfileNames.Remove(profile.DisplayName);
                        }
                    }
                    if (systemProfiles.Count > 0 && AppConfig.Current.NoUserProfiles)
                    {
                        Config.Clear();
                    }
                    if (Config.Any(x => x.IsDefault))
                    {
                        foreach (var systemProfile in systemProfiles)
                        {
                            systemProfile.IsDefault = false;
                        }
                    }
                    Config.InsertRange(0, systemProfiles);
                }
            }
        }
    }
}
