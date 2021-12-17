using System;
using NAPS2.Serialization;

namespace NAPS2.Config;

public static class ConfigScope
{
    public static ObjectConfigScope<T> Object<T>(T value) =>
        new ObjectConfigScope<T>(value, ConfigScopeMode.ReadWrite);

    public static ObjectConfigScope<T> Object<T>(T value, ConfigScopeMode mode) =>
        new ObjectConfigScope<T>(value, mode);

    public static FileConfigScope<T> File<T>(string filePath, Func<T> factory, ISerializer<T> serializer, ConfigScopeMode mode) =>
        new FileConfigScope<T>(filePath, factory, serializer, mode);
}

public abstract class ConfigScope<TConfig>
{
    protected ConfigScope(ConfigScopeMode mode)
    {
        Mode = mode;
    }

    public ConfigScopeMode Mode { get; }

    public T Get<T>(Func<TConfig, T> func)
    {
        lock (this)
        {
            return GetInternal(func);
        }
    }

    public void Set(Action<TConfig> func)
    {
        if (Mode == ConfigScopeMode.ReadOnly)
        {
            throw new NotSupportedException("This config scope is in ReadOnly mode.");
        }
        lock (this)
        {
            SetInternal(func);
        }
    }

    public void SetAll(TConfig changes)
    {
        if (Mode == ConfigScopeMode.ReadOnly)
        {
            throw new NotSupportedException("This config scope is in ReadOnly mode.");
        }
        lock (this)
        {
            SetAllInternal(changes);
        }
    }

    protected abstract T GetInternal<T>(Func<TConfig, T> func);

    protected abstract void SetInternal(Action<TConfig> func);

    protected abstract void SetAllInternal(TConfig delta);
}