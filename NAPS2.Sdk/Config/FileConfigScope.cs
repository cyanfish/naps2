using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NAPS2.Logging;
using NAPS2.Serialization;

namespace NAPS2.Config
{
    public class FileConfigScope<TConfig> : ConfigScope<TConfig>
    {
        private readonly string filePath;
        private readonly Func<TConfig> factory;
        private readonly ISerializer<TConfig> serializer;
        private TConfig cache;
        private TConfig changes;

        public FileConfigScope(string filePath, Func<TConfig> factory, ISerializer<TConfig> serializer, ConfigScopeMode mode) : base(mode)
        {
            this.filePath = filePath;
            this.factory = factory;
            this.serializer = serializer;
            cache = factory();
            changes = factory();
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

        protected override void SetAllInternal(TConfig delta)
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
                TConfig copy;
                using (var stream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
                {
                    // Reload cache so we don't overwrite concurrent changes
                    try
                    {
                        cache = serializer.Deserialize(stream);
                    }
                    catch (Exception)
                    {
                        // Failed to load. Since we're using FileShare.None, it can't be concurrent modification.
                        // Either the file is newly created, or it was corrupted. In either case we can ignore and overwrite.
                    }
                    // Merge the cache and our changes into a local copy (so in case of exceptions nothing is changed)
                    copy = factory();
                    ConfigCopier.Copy(cache, copy);
                    ConfigCopier.Copy(changes, copy);
                    // Try and write the changes
                    stream.Seek(0, SeekOrigin.Begin);
                    serializer.Serialize(stream, copy);
                    stream.SetLength(stream.Position);
                }
                // No exceptions, so we can commit the updated configuration and reset our changes
                cache = copy;
                changes = factory();
            }
            catch (IOException ex)
            {
                Log.ErrorException($"Error writing {filePath}", ex);
            }
        }
    }
}
