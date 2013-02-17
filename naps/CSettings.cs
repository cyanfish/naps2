/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009        Pavel Sorejs
    Copyright (C) 2012, 2013  Ben Olden-Cooligan

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

namespace NAPS
{
    class CSettings
    {
        private const string PROFILES_FILE = "profiles.xml";

        public static List<CScanSettings> LoadProfiles()
        {
            if (File.Exists(Application.StartupPath + "\\" + PROFILES_FILE))
            {
                Stream strFile = File.OpenRead(Application.StartupPath + "\\" + PROFILES_FILE);
                XmlSerializer serializer = new XmlSerializer(typeof(List<CScanSettings>));
                List<CScanSettings> ret = (List<CScanSettings>)serializer.Deserialize(strFile);
                strFile.Close();
                return ret;
            }
            else
            {
                return new List<CScanSettings>();
            }
        }

        public static void SaveProfiles(List<CScanSettings> profiles)
        {
            Stream strFile = File.Open(Application.StartupPath + "\\" + PROFILES_FILE,FileMode.Create);
            XmlSerializer serializer = new XmlSerializer(typeof(List<CScanSettings>));
            serializer.Serialize(strFile, profiles);
            strFile.Close();
        }
    }
}
