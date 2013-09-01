/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
    Copyright (C) 2013       Peter De Leeuw
    Copyright (C) 2012-2013  Ben Olden-Cooligan

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
using NAPS2.Scan.Wia;
using NLog;

namespace NAPS2.Config
{
    public class ProfileManager : ConfigManager<List<ExtendedScanSettings>>, IProfileManager
    {
        public ProfileManager()
            : base("profiles.xml", Paths.AppData, Paths.Executable, () => new List<ExtendedScanSettings>())
        {
        }

        public List<ExtendedScanSettings> Profiles { get { return Config; } }

        protected override List<ExtendedScanSettings> Deserialize(Stream configFileStream)
        {
            var serializer = new XmlSerializer(typeof(List<ExtendedScanSettings>));
            try
            {
                return (List<ExtendedScanSettings>)serializer.Deserialize(configFileStream);
            }
            catch (InvalidOperationException)
            {
                // Continue, and try to read using the old serializer now
                configFileStream.Seek(0, SeekOrigin.Begin);
            }

            // For compatibility with profiles.xml from old versions, load ScanSettings instead of ExtendedScanSettings (which is used exclusively now)
            var deprecatedSerializer = new XmlSerializer(typeof(List<ScanSettings>));
            var profiles = (List<ScanSettings>)deprecatedSerializer.Deserialize(configFileStream);

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
                if (!(profile is ExtendedScanSettings))
                {
                    // Everything should be ExtendedScanSettings now
                    return new ExtendedScanSettings
                    {
                        Version = ExtendedScanSettings.CURRENT_VERSION,
                        Device = profile.Device,
                        DriverName = profile.DriverName,
                        DisplayName = profile.DisplayName,
                        MaxQuality = profile.MaxQuality,
                        IsDefault = profile.IsDefault,
                        IconID = profile.IconID,
                        // If the driver is WIA and the profile type is not Extended, that meant the native UI was to be used
                        UseNativeUI = profile.DriverName == WiaScanDriver.DRIVER_NAME
                    };
                }
                return (ExtendedScanSettings)profile;
            }).ToList();
        }

        public void SetDefault(ExtendedScanSettings defaultProfile)
        {
            foreach (ExtendedScanSettings profile in Profiles)
            {
                profile.IsDefault = false;
            }
            defaultProfile.IsDefault = true;
        }
    }
}
