using System.Linq.Expressions;
using NAPS2.Serialization;

namespace NAPS2.Config.Model;

public class FileConfigScope<TConfig> : ConfigScope<TConfig>
{
    private readonly string _filePath;
    private readonly ISerializer<ConfigStorage<TConfig>> _serializer;
    private ConfigStorage<TConfig> _cache = new();
    private ConfigStorage<TConfig> _changes = new();

    public FileConfigScope(string filePath, ISerializer<ConfigStorage<TConfig>> serializer, ConfigScopeMode mode) : base(mode)
    {
        _filePath = filePath;
        _serializer = serializer;
    }

    protected override bool TryGetInternal(ConfigLookup lookup, out object? value)
    {
        // TODO: Use FileSystemWatcher to determine if we actually
        // TODO: need to read from disk. Also to create change events.
        ReadHandshake();
        if (_changes.TryGet(lookup, out value))
        {
            return true;
        }
        if (_cache.TryGet(lookup, out value))
        {
            return true;
        }
        return false;
    }

    protected override void SetInternal<T>(Expression<Func<TConfig, T>> accessor, T value)
    {
        _changes.Set(accessor, value);
        WriteHandshake();
    }

    protected override void RemoveInternal<T>(Expression<Func<TConfig, T>> accessor)
    {
        _changes.Remove(accessor);
        WriteHandshake();
    }

    protected override void CopyFromInternal(ConfigStorage<TConfig> source)
    {
        _changes.CopyFrom(source);
        WriteHandshake();
    }

    private void ReadHandshake()
    {
        // TODO: Retry
        try
        {
            using var stream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            _cache = _serializer.Deserialize(stream) ?? throw new InvalidOperationException();
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
            ConfigStorage<TConfig> copy;
            using (var stream = new FileStream(_filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
            {
                // Reload cache so we don't overwrite concurrent changes
                try
                {
                    _cache = _serializer.Deserialize(stream) ?? throw new InvalidOperationException();
                }
                catch (Exception)
                {
                    // Failed to load. Since we're using FileShare.None, it can't be concurrent modification.
                    // Either the file is newly created, or it was corrupted. In either case we can ignore and overwrite.
                }
                // Merge the cache and our changes into a local copy (so in case of exceptions nothing is changed)
                copy = new ConfigStorage<TConfig>();
                copy.CopyFrom(_cache);
                copy.CopyFrom(_changes);
                // Try and write the changes
                stream.Seek(0, SeekOrigin.Begin);
                _serializer.Serialize(stream, copy);
                stream.SetLength(stream.Position);
            }
            // No exceptions, so we can commit the updated configuration and reset our changes
            _cache = copy;
            _changes = new ConfigStorage<TConfig>();
        }
        catch (IOException ex)
        {
            Log.ErrorException($"Error writing {_filePath}", ex);
        }
    }
}