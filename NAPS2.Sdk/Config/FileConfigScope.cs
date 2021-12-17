using System;
using System.IO;
using NAPS2.Logging;
using NAPS2.Serialization;

namespace NAPS2.Config;

public class FileConfigScope<TConfig> : ConfigScope<TConfig>
{
    private readonly string _filePath;
    private readonly Func<TConfig> _factory;
    private readonly ISerializer<TConfig> _serializer;
    private TConfig _cache;
    private TConfig _changes;

    public FileConfigScope(string filePath, Func<TConfig> factory, ISerializer<TConfig> serializer, ConfigScopeMode mode) : base(mode)
    {
        _filePath = filePath;
        _factory = factory;
        _serializer = serializer;
        _cache = factory();
        _changes = factory();
    }

    protected override T GetInternal<T>(Func<TConfig, T> func)
    {
        // TODO: Use FileSystemWatcher to determine if we actually
        // TODO: need to read from disk. Also to create change events.
        ReadHandshake();
        var value = func(_changes);
        if (value != null)
        {
            return value;
        }
        return func(_cache);
    }

    protected override void SetInternal(Action<TConfig> func)
    {
        func(_changes);
        WriteHandshake();
    }

    protected override void SetAllInternal(TConfig delta)
    {
        ConfigCopier.Copy(delta, _changes);
        WriteHandshake();
    }

    private void ReadHandshake()
    {
        // TODO: Retry
        try
        {
            using var stream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            _cache = _serializer.Deserialize(stream);
        }
        catch (FileNotFoundException)
        {
        }
        catch (IOException ex)
        {
            Log.ErrorException($"Error reading {_filePath}", ex);
        }
    }

    private void WriteHandshake()
    {
        // TODO: Retry, maybe async?
        try
        {
            TConfig copy;
            using (var stream = new FileStream(_filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
            {
                // Reload cache so we don't overwrite concurrent changes
                try
                {
                    _cache = _serializer.Deserialize(stream);
                }
                catch (Exception)
                {
                    // Failed to load. Since we're using FileShare.None, it can't be concurrent modification.
                    // Either the file is newly created, or it was corrupted. In either case we can ignore and overwrite.
                }
                // Merge the cache and our changes into a local copy (so in case of exceptions nothing is changed)
                copy = _factory();
                ConfigCopier.Copy(_cache, copy);
                ConfigCopier.Copy(_changes, copy);
                // Try and write the changes
                stream.Seek(0, SeekOrigin.Begin);
                _serializer.Serialize(stream, copy);
                stream.SetLength(stream.Position);
            }
            // No exceptions, so we can commit the updated configuration and reset our changes
            _cache = copy;
            _changes = _factory();
        }
        catch (IOException ex)
        {
            Log.ErrorException($"Error writing {_filePath}", ex);
        }
    }
}