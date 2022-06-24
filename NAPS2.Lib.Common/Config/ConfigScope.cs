using System.Linq.Expressions;
using NAPS2.Serialization;

namespace NAPS2.Config;

public static class ConfigScope
{
    public static MemoryConfigScope<T> Memory<T>() => new();

    public static FileConfigScope<T> File<T>(string filePath, ISerializer<ConfigStorage<T>> serializer, ConfigScopeMode mode) =>
        new(filePath, serializer, mode);

    public static DefaultsConfigScope<T> Defaults<T>(T defaults) => new(defaults);
}

public abstract class ConfigScope<TConfig>
{
    protected ConfigScope(ConfigScopeMode mode)
    {
        Mode = mode;
    }

    public ConfigScopeMode Mode { get; }

    public bool TryGet<T>(Expression<Func<TConfig, T>> accessor, out T value)
    {
        var result = TryGet(ConfigLookup.ExpandExpression(accessor), out var obj);
        value = result ? (T) obj! : default!;
        return result;
    }

    public bool TryGet(ConfigLookup lookup, out object? value)
    {
        lock (this)
        {
            return TryGetInternal(lookup, out value);
        }
    }
    
    public T GetOr<T>(Expression<Func<TConfig, T>> accessor, T orValue)
    {
        return TryGet(accessor, out var value) ? value : orValue;
    }
    
    public T GetOrDefault<T>(Expression<Func<TConfig, T>> accessor) => GetOr(accessor, default);

    public void Set<T>(Expression<Func<TConfig, T>> accessor, T value)
    {
        if (Mode == ConfigScopeMode.ReadOnly)
        {
            throw new NotSupportedException("This config scope is in ReadOnly mode.");
        }
        lock (this)
        {
            SetInternal(accessor, value);
        }
    }

    public void Remove<T>(Expression<Func<TConfig, T>> accessor)
    {
        if (Mode == ConfigScopeMode.ReadOnly)
        {
            throw new NotSupportedException("This config scope is in ReadOnly mode.");
        }
        lock (this)
        {
            RemoveInternal(accessor);
        }
    }

    public void CopyFrom(ConfigStorage<TConfig> source)
    {
        if (Mode == ConfigScopeMode.ReadOnly)
        {
            throw new NotSupportedException("This config scope is in ReadOnly mode.");
        }
        lock (this)
        {
            CopyFromInternal(source);
        }
    }

    protected abstract bool TryGetInternal(ConfigLookup accessor, out object? value);

    // TODO: Maybe use ConfigLookup for consistency
    protected abstract void SetInternal<T>(Expression<Func<TConfig, T>> accessor, T value);

    protected abstract void RemoveInternal<T>(Expression<Func<TConfig, T>> accessor);

    protected abstract void CopyFromInternal(ConfigStorage<TConfig> source);
}