using System.Linq.Expressions;

namespace NAPS2.Config;

public class MemoryConfigScope<TConfig> : ConfigScope<TConfig>
{
    private readonly ConfigStorage<TConfig> _storage = new();

    public MemoryConfigScope() : base(ConfigScopeMode.ReadWrite)
    {
    }

    protected override bool TryGetInternal(ConfigLookup lookup, out object? value)
    {
        return _storage.TryGet(lookup, out value);
    }

    protected override void SetInternal<T>(Expression<Func<TConfig, T>> accessor, T value)
    {
        _storage.Set(accessor, value);
    }

    protected override void RemoveInternal<T>(Expression<Func<TConfig, T>> accessor)
    {
        _storage.Remove(accessor);
    }

    protected override void CopyFromInternal(ConfigStorage<TConfig> source)
    {
        _storage.CopyFrom(source);
    }
}