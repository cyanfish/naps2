using System.Collections.Generic;
using System.IO;
using System.Linq;
using NAPS2.Scan;
using NAPS2.Serialization;

namespace NAPS2.Config
{
    public class ProfileManager : IProfileManager
    {
        private readonly ISerializer<List<ScanProfile>> serializer = new XmlSerializer<List<ScanProfile>>();
        private readonly string userPath;
        private readonly string systemPath;
        private readonly bool lockSystemProfiles;
        private readonly bool lockUnspecifiedDevices;
        private readonly bool noUserProfiles;

        private List<ScanProfile> profiles;

        public ProfileManager(string userPath, string systemPath, bool lockSystemProfiles, bool lockUnspecifiedDevices, bool noUserProfiles)
        {
            this.userPath = userPath;
            this.systemPath = systemPath;
            this.lockSystemProfiles = lockSystemProfiles;
            this.lockUnspecifiedDevices = lockUnspecifiedDevices;
            this.noUserProfiles = noUserProfiles;
        }

        public List<ScanProfile> Profiles
        {
            get
            {
                if (profiles == null)
                {
                    Load();
                }
                return profiles;
            }
        }

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

        public void Load()
        {
            if (File.Exists(userPath))
            {
                profiles = serializer.DeserializeFromFile(userPath);
                if (lockSystemProfiles && File.Exists(systemPath))
                {
                    LoadLockedProfiles();
                }
            }
            else if (File.Exists(systemPath))
            {
                profiles = serializer.DeserializeFromFile(systemPath);
            }
            else
            {
                profiles = new List<ScanProfile>();
            }
        }

        private void LoadLockedProfiles()
        {
            var systemProfiles = serializer.DeserializeFromFile(systemPath);
            foreach (var systemProfile in systemProfiles)
            {
                systemProfile.IsLocked = true;
                systemProfile.IsDeviceLocked = (systemProfile.Device != null || lockUnspecifiedDevices);
            }

            var systemProfileNames = new HashSet<string>(systemProfiles.Select(x => x.DisplayName));
            foreach (var profile in profiles)
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
                    profiles.Remove(profile);
                    // Avoid removing duplicates
                    systemProfileNames.Remove(profile.DisplayName);
                }
            }
            if (systemProfiles.Count > 0 && noUserProfiles)
            {
                profiles.Clear();
            }
            if (profiles.Any(x => x.IsDefault))
            {
                foreach (var systemProfile in systemProfiles)
                {
                    systemProfile.IsDefault = false;
                }
            }
            profiles.InsertRange(0, systemProfiles);
        }

        public void Save()
        {
            if (profiles == null)
            {
                Load();
            }
            serializer.SerializeToFile(userPath, profiles);
        }
    }
}
