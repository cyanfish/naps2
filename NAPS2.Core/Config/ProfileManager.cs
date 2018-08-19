using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using NAPS2.Scan;
using NAPS2.Scan.Twain;
using NAPS2.Scan.Wia;

namespace NAPS2.Config
{
    public class ProfileManager : ConfigManager<List<ScanProfile>>, IProfileManager
    {
        private readonly AppConfigManager appConfigManager;

        public ProfileManager(AppConfigManager appConfigManager)
            : base("profiles.xml", Paths.AppData, Paths.Executable, () => new List<ScanProfile>())
        {
            this.appConfigManager = appConfigManager;
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
            if (appConfigManager.Config.LockSystemProfiles)
            {
                var systemProfiles = TryLoadConfig(secondaryConfigPath);
                if (systemProfiles != null)
                {
                    foreach (var systemProfile in systemProfiles)
                    {
                        systemProfile.IsLocked = true;
                        systemProfile.IsDeviceLocked = (systemProfile.Device != null || appConfigManager.Config.LockUnspecifiedDevices);
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
                    if (systemProfiles.Count > 0 && appConfigManager.Config.NoUserProfiles)
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

        protected override List<ScanProfile> Deserialize(Stream configFileStream)
        {
            try
            {
                return ReadProfiles(configFileStream);
            }
            catch (InvalidOperationException)
            {
                // Continue, and try to read using the old serializer now
                configFileStream.Seek(0, SeekOrigin.Begin);
            }

            try
            {
                return ReadOldProfiles(configFileStream);
            }
            catch (InvalidOperationException)
            {
                // Continue, and try to read using the older serializer now
                configFileStream.Seek(0, SeekOrigin.Begin);
            }

            return ReadVeryOldProfiles(configFileStream);
        }

        private static List<ScanProfile> ReadProfiles(Stream configFileStream)
        {
            var serializer = new XmlSerializer(typeof(List<ScanProfile>));
            var settingsList = (List<ScanProfile>)serializer.Deserialize(configFileStream);
            // Upgrade from v1 to v2 if necessary
            foreach (var settings in settingsList)
            {
                if (settings.Version == 1)
                {
                    if (settings.DriverName == TwainScanDriver.DRIVER_NAME)
                    {
                        settings.UseNativeUI = true;
                    }
                    settings.Version = ScanProfile.CURRENT_VERSION;
                }
            }
            return settingsList;
        }

        private static List<ScanProfile> ReadOldProfiles(Stream configFileStream)
        {
            var serializer = new XmlSerializer(typeof(List<OldExtendedScanSettings>));
            var profiles = (List<OldExtendedScanSettings>)serializer.Deserialize(configFileStream);
            // Upgrade from v1 to v2 if necessary
            foreach (var settings in profiles)
            {
                if (settings.Version == 1)
                {
                    if (settings.DriverName == TwainScanDriver.DRIVER_NAME)
                    {
                        settings.UseNativeUI = true;
                    }
                    settings.Version = ScanProfile.CURRENT_VERSION;
                }
            }
            return profiles.Select(profile => new ScanProfile
            {
                Version = ScanProfile.CURRENT_VERSION,
                Device = profile.Device,
                DriverName = profile.DriverName,
                DisplayName = profile.DisplayName,
                MaxQuality = profile.MaxQuality,
                IsDefault = profile.IsDefault,
                IconID = profile.IconID,
                AfterScanScale = profile.AfterScanScale,
                BitDepth = profile.BitDepth,
                Brightness = profile.Brightness,
                Contrast = profile.Contrast,
                CustomPageSize = profile.CustomPageSize,
                PageAlign = profile.PageAlign,
                PageSize = profile.PageSize,
                PaperSource = profile.PaperSource,
                Resolution = profile.Resolution,
                UseNativeUI = profile.UseNativeUI
            }).ToList();
        }

        private List<ScanProfile> ReadVeryOldProfiles(Stream configFileStream)
        {
            // For compatibility with profiles.xml from old versions, load OldScanSettings instead of ScanProfile (which is used exclusively now)
            var deprecatedSerializer = new XmlSerializer(typeof (List<OldScanSettings>));
            var profiles = (List<OldScanSettings>) deprecatedSerializer.Deserialize(configFileStream);

            // Okay, we've read the old version of profiles.txt. Since we're going to eventually change it to the new version, make a backup in case the user downgrades.
            File.Copy(primaryConfigPath, primaryConfigPath + ".bak", true);

            return profiles.Select(profile =>
            {
                if (profile.DriverName == null && profile.Device != null)
                {
                    // Copy the DriverName to the new property
                    profile.DriverName = profile.Device.DriverName;
                    // This old property is unused, so remove its value
                    profile.Device.DriverName = null;
                }
                // Everything should be ScanProfile now
                var result = new ScanProfile
                {
                    Version = ScanProfile.CURRENT_VERSION,
                    Device = profile.Device,
                    DriverName = profile.DriverName,
                    DisplayName = profile.DisplayName,
                    MaxQuality = profile.MaxQuality,
                    IsDefault = profile.IsDefault,
                    IconID = profile.IconID,
                    // If the driver is WIA and the profile type is not Extended, that meant the native UI was to be used
                    UseNativeUI = profile.DriverName == WiaScanDriver.DRIVER_NAME
                };
                if (profile is OldExtendedScanSettings ext)
                {
                    result.AfterScanScale = ext.AfterScanScale;
                    result.BitDepth = ext.BitDepth;
                    result.Brightness = ext.Brightness;
                    result.Contrast = ext.Contrast;
                    result.CustomPageSize = ext.CustomPageSize;
                    result.PageAlign = ext.PageAlign;
                    result.PageSize = ext.PageSize;
                    result.PaperSource = ext.PaperSource;
                    result.Resolution = ext.Resolution;
                    result.UseNativeUI = ext.UseNativeUI;
                }
                return result;
            }).ToList();
        }
    }
}
