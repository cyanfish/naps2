/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
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
using System.Windows.Forms;
using System.Xml.Serialization;
using NAPS2.Scan;

namespace NAPS2
{
    public class ProfileManager : IProfileManager
    {
        private const string ProfilesFileName = "profiles.xml";
#if STANDALONE
        private static readonly string ProfilesFolder = Application.StartupPath;
#else
        private static readonly string ProfilesFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NAPS2");
#endif
        private static readonly string ProfilesPath = Path.Combine(ProfilesFolder, ProfilesFileName);

        private static readonly string OldProfilesPath = Path.Combine(Application.StartupPath, "profiles.xml");

        private List<ScanSettings> profiles;

        public List<ScanSettings> Profiles
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

        public void Load()
        {
            profiles = null;
            TryLoadProfiles(ProfilesPath);
            if (profiles == null)
            {
                // Try migrating from an older version
                TryLoadProfiles(OldProfilesPath);
                if (profiles != null)
                {
                    Save();
                    try
                    {
                        File.Delete(OldProfilesPath);
                    }
                    catch (IOException) { }
                }
            }
            if (profiles == null)
            {
                profiles = new List<ScanSettings>();
                Save();
            }
        }

        public void Save()
        {
            if (!Directory.Exists(ProfilesFolder))
            {
                Directory.CreateDirectory(ProfilesFolder);
            }
            using (Stream strFile = File.Open(ProfilesPath, FileMode.Create))
            {
                var serializer = new XmlSerializer(typeof(List<ScanSettings>));
                serializer.Serialize(strFile, profiles);
            }
        }

        private void TryLoadProfiles(string profilesPath)
        {
            profiles = null;
            if (File.Exists(profilesPath))
            {
                try
                {
                    using (Stream strFile = File.OpenRead(profilesPath))
                    {
                        var serializer = new XmlSerializer(typeof(List<ScanSettings>));
                        profiles = (List<ScanSettings>)serializer.Deserialize(strFile);
                    }
                }
                catch (Exception) { }
            }
        }
    }
}
