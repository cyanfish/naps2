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
using NLog;

namespace NAPS2.Config
{
    public abstract class ConfigManager<T> where T : class
    {
        protected readonly string primaryConfigPath;
        protected readonly string secondaryConfigPath;

        private readonly Logger logger;
        private readonly Func<T> factory;

        private T config;

        public ConfigManager(string indexFileName, string recoveryFolderPath, string secondaryFolder, Func<T> factory)
        {
            primaryConfigPath = Path.Combine(recoveryFolderPath, indexFileName);
            if (secondaryFolder != null)
            {
                secondaryConfigPath = Path.Combine(secondaryFolder, indexFileName);
            }
            this.factory = factory;
            logger = LoggerFactory.Current.GetLogger();
        }

        protected T Config
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

        public virtual void Load()
        {
            config = null;
            TryLoadConfig(primaryConfigPath);
            if (config == null && secondaryConfigPath != null)
            {
                TryLoadConfig(secondaryConfigPath);
            }
            if (config == null)
            {
                config = factory();
            }
        }

        public void Save()
        {
            using (Stream strFile = File.Open(primaryConfigPath, FileMode.Create))
            {
                var serializer = new XmlSerializer(typeof(T));
                // TODO: Rather than overwrite, do the write-to-temp/move song-and-dance to avoid corruption
                serializer.Serialize(strFile, config);
            }
        }

        protected virtual T Deserialize(Stream configFileStream)
        {
            var serializer = new XmlSerializer(typeof(T));
            return (T)serializer.Deserialize(configFileStream);
        }

        private void TryLoadConfig(string configPath)
        {
            config = null;
            if (File.Exists(configPath))
            {
                try
                {
                    using (Stream configFileStream = File.OpenRead(configPath))
                    {
                        config = Deserialize(configFileStream);
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
