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
using NLog;

namespace NAPS2
{
    public class UserConfigManager
    {
        private const string CONFIG_FILE_NAME = "config.xml";

        private static readonly string ConfigPath = Path.Combine(Paths.AppData, CONFIG_FILE_NAME);

        private readonly Logger logger;

        private UserConfig config;

        public UserConfigManager(Logger logger)
        {
            this.logger = logger;
        }

        public UserConfig Config
        {
            get
            {
                if (config == null)
                {
                    Load();
                }
                return config;
            }
        }

        public void Load()
        {
            config = null;
            TryLoadConfig(ConfigPath);
            if (config == null)
            {
                config = new UserConfig();
                Save();
            }
        }

        public void Save()
        {
            using (Stream strFile = File.Open(ConfigPath, FileMode.Create))
            {
                var serializer = new XmlSerializer(typeof(UserConfig));
                serializer.Serialize(strFile, config);
            }
        }

        private void TryLoadConfig(string configPath)
        {
            config = null;
            if (File.Exists(configPath))
            {
                try
                {
                    using (Stream strFile = File.OpenRead(configPath))
                    {
                        var serializer = new XmlSerializer(typeof (UserConfig));
                        config = (UserConfig)serializer.Deserialize(strFile);
                    }
                }
                catch (Exception ex)
                {
                    logger.ErrorException("Error loading config.", ex);
                }
            }
        }
    }
}
