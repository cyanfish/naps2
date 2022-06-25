using System.Linq.Expressions;

namespace NAPS2.Config.Model;

public abstract class ScopedConfig<TConfig>
{
    protected ConfigScope<TConfig>[] Scopes { get; init; }

    public T Get<T>(Expression<Func<TConfig, T>> accessor)
    {
        var lookup = ConfigLookup.ExpandExpression(accessor);
        return (T) Get(lookup);
    }

    private object? Get(ConfigLookup lookup)
    {
        if (!lookup.Tail.IsLeaf)
        {
            var obj = Activator.CreateInstance(lookup.Tail.Type);
            FillObject(obj, lookup);
            return obj;
        }
        foreach (var scope in Scopes)
        {
            if (scope.TryGet(lookup, out var value))
            {
                return value;
            }
        }
        // This shouldn't happen - the last config scope should always define a default value for every property.
        throw new Exception("Config value not defined.");
    }

    private void FillObject(object? obj, ConfigLookup lookup)
    {
        foreach (var prop in ConfigLookup.GetPropertyData(lookup.Tail.Type))
        {
            var subLookup = lookup.Append(prop);
            if (prop.IsNestedConfig)
            {
                FillObject(prop.PropertyInfo.GetValue(obj), subLookup);
            }
            else
            {
                prop.PropertyInfo.SetValue(obj, Get(subLookup));
            }
        }
    }
}