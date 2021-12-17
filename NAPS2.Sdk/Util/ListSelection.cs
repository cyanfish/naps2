using System.Collections;

namespace NAPS2.Util;

// TODO: Move this to another namespace
public static class ListSelection
{
    public static ListSelection<T> From<T>(IEnumerable<T> list) =>
        new(list);

    public static ListSelection<T> Of<T>(params T[] items) =>
        new(items);

    public static ListSelection<T> FromSelectedIndices<T>(IList<T> list, IEnumerable<int> selectedIndices) =>
        new(list.ElementsAt(selectedIndices));

    public static ListSelection<T> Empty<T>() =>
        new(Enumerable.Empty<T>());
}

public class ListSelection<T> : IEnumerable<T>, IEquatable<ListSelection<T>>
{
    private readonly HashSet<T> _internalSelection;

    public ListSelection(IEnumerable<T> selectedItems)
    {
        _internalSelection = new HashSet<T>(selectedItems);
    }

    public IEnumerable<int> ToSelectedIndices(List<T> list) => list.IndiciesOf(_internalSelection);

    public int Count => _internalSelection.Count;
        
    public bool Contains(T item) => _internalSelection.Contains(item);

    public IEnumerator<T> GetEnumerator() => _internalSelection.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public bool Equals(ListSelection<T>? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return _internalSelection.SetEquals(other._internalSelection);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((ListSelection<T>) obj);
    }

    public override int GetHashCode() => _internalSelection.GetHashCode();

    public static bool operator ==(ListSelection<T> left, ListSelection<T> right) => Equals(left, right);

    public static bool operator !=(ListSelection<T> left, ListSelection<T> right) => !Equals(left, right);
}

public interface ISelectable<T>
{
    ListSelection<T> Selection { get; set; }
}
    
public class Selectable<T>
{
    private ListSelection<T> _selection = ListSelection.Empty<T>();

    public ListSelection<T> Selection
    {
        get => _selection;
        set => _selection = value ?? throw new ArgumentNullException(nameof(value));
    }
}