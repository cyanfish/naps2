using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NAPS2.Logging;
using NAPS2.Serialization;
using NAPS2.Util;

namespace NAPS2.Config.Experimental
{
    public class FileConfigScope<TConfig> : ConfigScope<TConfig> where TConfig : new()
    {
        private readonly string filePath;
        private readonly ISerializer<TConfig> serializer;
        private TConfig cache;
        private TConfig changes;

        public FileConfigScope(string filePath, ISerializer<TConfig> serializer, ConfigScopeMode mode) : base(mode)
        {
            this.filePath = filePath;
            this.serializer = serializer;
            cache = new TConfig();
            changes = new TConfig();
        }

        protected override T GetInternal<T>(Func<TConfig, T> func)
        {
            // TODO: Use FileSystemWatcher to determine if we actually
            // TODO: need to read from disk. Also to create change events.
            ReadHandshake();
            var value = func(changes);
            if (value != null)
            {
                return value;
            }
            return func(cache);
        }

        protected override void SetInternal(Action<TConfig> func)
        {
            func(changes);
            WriteHandshake();
        }

        public override void SetAllInternal(TConfig delta)
        {
            ConfigCopier.Copy(delta, changes);
            WriteHandshake();
        }

        private void ReadHandshake()
        {
            // TODO: Retry
            try
            {
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    cache = serializer.Deserialize(stream);
                }
            }
            catch (FileNotFoundException)
            {
            }
            catch (IOException ex)
            {
                Log.ErrorException($"Error reading {filePath}", ex);
            }
        }

        private void WriteHandshake()
        {
            // TODO: Retry, maybe async?
            try
            {
                using (var stream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
                {
                    cache = serializer.Deserialize(stream);
                    var copy = ConfigCopier.Copy(cache);
                    serializer.Serialize(stream, copy);
                    cache = copy;
                    changes = new TConfig();
                }
            }
            catch (IOException ex)
            {
                Log.ErrorException($"Error writing {filePath}", ex);
            }
        }
    }
}
