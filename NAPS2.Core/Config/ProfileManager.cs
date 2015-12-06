/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
    Copyright (C) 2013       Peter De Leeuw
    Copyright (C) 2012-2015  Ben Olden-Cooligan

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
*/

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
        public ProfileManager()
            : base("profiles.xml", Paths.AppData, Paths.Executable, () => new List<ScanProfile>())
        {
        }

        public List<ScanProfile> Profiles { get { return Config; } }

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
                var ext = profile as OldExtendedScanSettings;
                if (ext != null)
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
