namespace NAPS2.Util;

public class DisposableSet<T> : IDisposable where T : IDisposable
{
    private readonly HashSet<T> _set = [];

    public void Add(T obj)
    {
        lock (this)
        {
            _set.Add(obj);
        }
    }
    
    public void Remove(T obj)
    {
        lock (this)
        {
            _set.Remove(obj);
        }
    }

    public void Dispose()
    {
        lock (this)
        {
            foreach (var item in _set)
            {
                item.Dispose();
            }
        }
    }
}