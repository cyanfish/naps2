using System.Windows;

namespace NAPS2.EtoForms.Wpf;

/// <summary>
/// A resource dictionary with implicit styles for the given types, which are only created when they're first looked
/// up. If no style is created for a type, the lookup falls through to the next dictionary.
/// </summary>
internal class LazyStyleDictionary : ResourceDictionary
{
    private static readonly object Placeholder = new();

    private readonly Func<Type, Style?> _createStyle;

    public LazyStyleDictionary(IEnumerable<Type> types, Func<Type, Style?> createStyle)
    {
        _createStyle = createStyle;
        foreach (var type in types)
        {
            Add(type, Placeholder);
        }
    }

    protected override void OnGettingValue(object key, ref object value, out bool canCache)
    {
        if (value == Placeholder)
        {
            value = _createStyle((Type) key)!;
            canCache = true;
            return;
        }
        base.OnGettingValue(key, ref value, out canCache);
    }
}
