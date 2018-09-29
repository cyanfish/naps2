using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using NAPS2.Logging;
using NAPS2.Util;

namespace NAPS2.Config
{
    public abstract class ConfigManager<T> where T : class
    {
        protected readonly string primaryConfigPath;
        protected readonly string secondaryConfigPath;

        private readonly Func<T> factory;

        private T config;

        protected ConfigManager(string indexFileName, string recoveryFolderPath, string secondaryFolder, Func<T> factory)
        {
            primaryConfigPath = Path.Combine(recoveryFolderPath, indexFileName);
            if (secondaryFolder != null)
            {
                secondaryConfigPath = Path.Combine(secondaryFolder, indexFileName);
            }
            this.factory = factory;
        }

        protected T Config
        {
            get
            {
                if (config == null)
                {
                    lock (this)
                    {
                        if (config == null) Load();
                    }
                }
                return config;
            }
        }

        public virtual void Load()
        {
            lock (this)
            {
                config = null;
                config = TryLoadConfig(primaryConfigPath);
                if (config == null && secondaryConfigPath != null)
                {
                    config = TryLoadConfig(secondaryConfigPath);
                }

                if (config == null)
                {
                    config = factory();
                }
            }
        }

        public void Save()
        {
            lock (this)
            {
                using (Stream strFile = File.Open(primaryConfigPath, FileMode.Create))
                {
                    var serializer = new XmlSerializer(typeof(T));
                    // TODO: Rather than overwrite, do the write-to-temp/move song-and-dance to avoid corruption
                    serializer.Serialize(strFile, config);
                }
            }
        }

        protected virtual T Deserialize(Stream configFileStream)
        {
            var serializer = new XmlSerializer(typeof(T));
            return (T)serializer.Deserialize(configFileStream);
        }

        protected T TryLoadConfig(string configPath)
        {
            if (File.Exists(configPath))
            {
                try
                {
                    using (Stream configFileStream = File.OpenRead(configPath))
                    {
                        return Deserialize(configFileStream);
                    }
                }
                catch (Exception ex)
                {
                    Log.ErrorException("Error loading config.", ex);
                }
            }
            return null;
        }
    }
}
