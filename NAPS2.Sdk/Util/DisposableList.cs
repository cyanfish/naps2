using System.Collections.Immutable;

namespace NAPS2.Util;

// TODO: Maybe instead of a generic DisposableList we should create an class for a list of ProcessedImage that has
// explicit ownership semantics
public class DisposableList<T> : IDisposable where T : IDisposable
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
}