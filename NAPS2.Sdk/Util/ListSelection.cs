using System.Collections;

namespace NAPS2.Util;

// TODO: Move this to another namespace
public static class ListSelection
{
    public static ListSelection<T> From<T>(IEnumerable<T> list) where T : notnull =>
        new(list);

    public static ListSelection<T> Of<T>(params T[] items) where T : notnull =>
        new(items);

    public static ListSelection<T> FromSelectedIndices<T>(IList<T> list, IEnumerable<int> selectedIndices)
        where T : notnull =>
        new(list.ElementsAt(selectedIndices));

    public static ListSelection<T> Empty<T>() where T : notnull =>
        new(Enumerable.Empty<T>());
}

public class ListSelection<T> : ICollection<T>, IEquatable<ListSelection<T>> where T : notnull
{
    private readonly HashSet<T> _internalSelection;

    public ListSelection(IEnumerable<T> selectedItems)
    {
        _internalSelection = new HashSet<T>(selectedItems);
    }

    public IEnumerable<int> ToSelectedIndices(List<T> list) => list.IndiciesOf(_internalSelection);

    public int Count => _internalSelection.Count;

    public bool Contains(T item) => _internalSelection.Contains(item);

    public bool IsReadOnly => true;
    public void Add(T item) => throw new NotSupportedException();
    public void Clear() => throw new NotSupportedException();
    public bool Remove(T item) => throw new NotSupportedException();

    public void CopyTo(T[] array, int arrayIndex) => _internalSelection.CopyTo(array, arrayIndex);

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

public interface ISelectable<T> where T : notnull
{
    ListSelection<T> Selection { get; set; }
}

public class Selectable<T> : ISelectable<T> where T : notnull
{
    private ListSelection<T> _selection = ListSelection.Empty<T>();

    public ListSelection<T> Selection
    {
        get => _selection;
        set => _selection = value ?? throw new ArgumentNullException(nameof(value));
    }
}