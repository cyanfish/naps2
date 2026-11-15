using System.Reflection;

namespace NAPS2.Serialization;

/// <summary>
/// Marks a type to have property accessors generated at compile time, which lets serialization read and write its
/// properties without reflection. Properties that can't be accessed from generated code (e.g. init-only or private
/// setters) still use reflection.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public sealed class GeneratePropertyAccessorsAttribute : Attribute;

/// <summary>
/// A generated getter and setter for a property. Either can be null if it isn't accessible from generated code.
/// </summary>
public sealed class PropertyAccessor(string name, Func<object, object?>? getter, Action<object, object?>? setter)
{
    public string Name { get; } = name;

    public Func<object, object?>? Getter { get; } = getter;

    public Action<object, object?>? Setter { get; } = setter;
}

/// <summary>
/// Provides getters and setters for properties, using generated accessors (see
/// <see cref="GeneratePropertyAccessorsAttribute"/>) when available and reflection otherwise.
/// </summary>
public static class PropertyAccessors
{
    private static readonly Dictionary<Type, Func<PropertyAccessor[]>> Factories = new();
    private static readonly Dictionary<Type, Dictionary<string, PropertyAccessor>> Cache = new();

    /// <summary>
    /// Registers the generated accessors for a type. This is called by generated code.
    /// </summary>
    public static void Register(Type type, Func<PropertyAccessor[]> factory)
    {
        lock (Factories)
        {
            Factories[type] = factory;
        }
    }

    public static Func<object, object?> GetGetter(PropertyInfo property)
    {
        return Find(property)?.Getter ?? property.GetValue;
    }

    public static Action<object, object?> GetSetter(PropertyInfo property)
    {
        return Find(property)?.Setter ?? property.SetValue;
    }

    private static PropertyAccessor? Find(PropertyInfo property)
    {
        var type = property.DeclaringType;
        if (type == null)
        {
            return null;
        }
        lock (Factories)
        {
            if (!Cache.TryGetValue(type, out var accessors))
            {
                accessors = Factories.TryGetValue(type, out var factory)
                    ? factory().ToDictionary(x => x.Name)
                    : new Dictionary<string, PropertyAccessor>();
                Cache[type] = accessors;
            }
            return accessors.TryGetValue(property.Name, out var accessor) ? accessor : null;
        }
    }
}
