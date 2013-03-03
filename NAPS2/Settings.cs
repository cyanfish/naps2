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
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Windows.Forms;
using NAPS2.Scan;

namespace NAPS2
{
    public class Settings
    {
        private static readonly string ProfilesFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NAPS2");
        private static readonly string ProfilesFileName = "profiles.xml";
        private static readonly string ProfilesPath = Path.Combine(ProfilesFolder, ProfilesFileName);

        public static List<ScanSettings> LoadProfiles()
        {
            if (File.Exists(ProfilesPath))
            {
                try
                {
                    using (Stream strFile = File.OpenRead(ProfilesPath))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(List<ScanSettings>));
                        return (List<ScanSettings>)serializer.Deserialize(strFile);
                    }
                }
                catch (Exception) { }
            }
            return new List<ScanSettings>();
        }

        public static void SaveProfiles(List<ScanSettings> profiles)
        {
            using (Stream strFile = File.Open(ProfilesPath, FileMode.Create))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<ScanSettings>));
                serializer.Serialize(strFile, profiles);
            }
        }
    }
}
