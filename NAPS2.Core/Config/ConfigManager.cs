using NAPS2.Util;
using System;
using System.IO;
using System.Xml.Serialization;

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
                    Load();
                }
                return config;
            }
        }

        public virtual void Load()
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