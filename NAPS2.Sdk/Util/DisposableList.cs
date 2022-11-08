using System.Collections;
using System.Collections.Immutable;

namespace NAPS2.Util;

// TODO: Maybe instead of a generic DisposableList we should create an class for a list of ProcessedImage that has
// explicit ownership semantics
public class DisposableList<T> : IDisposable, IList<T> where T : IDisposable
{
    public DisposableList(ImmutableList<T> innerList)
    {
        InnerList = innerList;
    }

    public ImmutableList<T> InnerList { get; }

    public void Dispose()
    {
        foreach(var item in InnerList)
        {
            item.Dispose();
        }
    }

    public IEnumerator<T> GetEnumerator() => InnerList.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => InnerList.GetEnumerator();
    public void Add(T item) => throw new NotSupportedException();
    public void Clear() => throw new NotSupportedException();
    public bool Contains(T item) => InnerList.Contains(item);
    public void CopyTo(T[] array, int arrayIndex) => InnerList.CopyTo(array, arrayIndex);
    public bool Remove(T item) => throw new NotSupportedException();
    public int Count => InnerList.Count;
    public bool IsReadOnly => true;
    public int IndexOf(T item) => InnerList.IndexOf(item);
    public void Insert(int index, T item) => throw new NotSupportedException();
    public void RemoveAt(int index) => throw new NotSupportedException();

    public T this[int index]
    {
        get => InnerList[index];
        set => throw new NotSupportedException();
    }
}