using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using NAPS2.Images.Transforms;
using NAPS2.Logging;
using NAPS2.Util;

namespace NAPS2.Config
{
    public class ConfigManager<T> : IConfigManager<T> where T : class
    {
        protected readonly string primaryConfigPath;
        protected readonly string secondaryConfigPath;

        private readonly Func<T> factory;
        private readonly ISerializer<T> serializer;

        private T config;

        public ConfigManager(string indexFileName, string primaryFolder, string secondaryFolder, Func<T> factory, ISerializer<T> serializer)
        {
            primaryConfigPath = Path.Combine(primaryFolder, indexFileName);
            if (secondaryFolder != null)
            {
                secondaryConfigPath = Path.Combine(secondaryFolder, indexFileName);
            }
            this.factory = factory;
            this.serializer = serializer;
        }

        public T Config
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
                // TODO: Rather than overwrite, do the write-to-temp/move song-and-dance to avoid corruption
                serializer.SerializeToFile(primaryConfigPath, config);
            }
        }

        protected T TryLoadConfig(string configPath)
        {
            if (File.Exists(configPath))
            {
                try
                {
                    return serializer.DeserializeFromFile(configPath);
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
