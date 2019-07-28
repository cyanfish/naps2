using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using NAPS2.Images;
using NAPS2.Scan;
using NAPS2.Serialization;

namespace NAPS2.Config
{
    public class ProfileManager : IProfileManager
    {
        private readonly ISerializer<ProfileConfig> serializer = new ProfileSerializer();
        private readonly FileConfigScope<ProfileConfig> userScope;
        private readonly FileConfigScope<ProfileConfig> appScope;
        private readonly bool userPathExisted;
        private readonly bool lockSystemProfiles;
        private readonly bool lockUnspecifiedDevices;
        private readonly bool noUserProfiles;

        private List<ScanProfile> profiles;

        public ProfileManager(string userPath, string systemPath, bool lockSystemProfiles, bool lockUnspecifiedDevices, bool noUserProfiles)
        {
            userPathExisted = File.Exists(userPath);
            userScope = ConfigScope.File(userPath, () => new ProfileConfig(), serializer, ConfigScopeMode.ReadWrite);
            appScope = ConfigScope.File(systemPath, () => new ProfileConfig(), serializer, ConfigScopeMode.ReadOnly);
            this.lockSystemProfiles = lockSystemProfiles;
            this.lockUnspecifiedDevices = lockUnspecifiedDevices;
            this.noUserProfiles = noUserProfiles;
        }

        public event EventHandler ProfilesUpdated;

        public ImmutableList<ScanProfile> Profiles
        {
            get
            {
                lock (this)
                {
                    Load();
                    return ImmutableList.CreateRange(profiles);
                }
            }
        }

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
            get
            {
                lock (this)
                {
                    Load();
                    if (profiles.Count == 1)
                    {
                        return profiles.First();
                    }
                    return profiles.FirstOrDefault(x => x.IsDefault);
                }
            }
            set
            {
                lock (this)
                {
                    Load();
                    foreach (var profile in profiles)
                    {
                        profile.IsDefault = false;
                    }
                    value.IsDefault = true;
                    Save();
                }
            }
        }

        public void Load()
        {
            lock (this)
            {
                if (profiles != null)
                {
                    return;
                }
                profiles = GetProfiles();
            }
        }

        public void Save()
        {
            lock (this)
            {
                userScope.Set(c => c.Profiles = ImmutableList.CreateRange(profiles));
            }
            ProfilesUpdated?.Invoke(this, EventArgs.Empty);
        }

        private List<ScanProfile> GetProfiles()
        {
            var userProfiles = (userScope.Get(c => c.Profiles) ?? ImmutableList<ScanProfile>.Empty).ToList();
            var systemProfiles = (appScope.Get(c => c.Profiles) ?? ImmutableList<ScanProfile>.Empty).ToList();
            if (noUserProfiles && systemProfiles.Count > 0)
            {
                // Configured by administrator to only use system profiles
                // But the user might still be able to change devices
                MergeUserProfilesIntoSystemProfiles(userProfiles, systemProfiles);
                return systemProfiles;
            }
            if (!userPathExisted)
            {
                // Initialize with system profiles since it's a new user
                return systemProfiles;
            }
            if (!lockSystemProfiles)
            {
                // Ignore the system profiles since the user already has their own
                return userProfiles;
            }
            // LockSystemProfiles has been specified, so we need both user and system profiles.
            MergeUserProfilesIntoSystemProfiles(userProfiles, systemProfiles);
            if (userProfiles.Any(x => x.IsDefault))
            {
                foreach (var systemProfile in systemProfiles)
                {
                    systemProfile.IsDefault = false;
                }
            }
            return systemProfiles.Concat(userProfiles).ToList();
        }

        private void MergeUserProfilesIntoSystemProfiles(List<ScanProfile> userProfiles, List<ScanProfile> systemProfiles)
        {
            foreach (var systemProfile in systemProfiles)
            {
                systemProfile.IsLocked = true;
                systemProfile.IsDeviceLocked = (systemProfile.Device != null || lockUnspecifiedDevices);
            }

            var systemProfileNames = new HashSet<string>(systemProfiles.Select(x => x.DisplayName));
            foreach (var profile in userProfiles)
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
                    userProfiles.Remove(profile);
                    // Avoid removing duplicates
                    systemProfileNames.Remove(profile.DisplayName);
                }
            }
        }

        private class ProfileConfig
        {
            public ImmutableList<ScanProfile> Profiles { get; set; }
        }

        private class ProfileSerializer : ISerializer<ProfileConfig>
        {
            private readonly XmlSerializer<ImmutableList<ScanProfile>> internalSerializer = new XmlSerializer<ImmutableList<ScanProfile>>();

            public void Serialize(Stream stream, ProfileConfig obj) => internalSerializer.Serialize(stream, obj.Profiles);

            public ProfileConfig Deserialize(Stream stream) => new ProfileConfig { Profiles = internalSerializer.Deserialize(stream) };
        }
    }
}
