// ReSharper disable once CheckNamespace
namespace NAPS2.Util;

public abstract class AsyncSource<T> where T : class
{
    public static AsyncSource<T> Empty => new EmptySource();

    public abstract Task<T?> Next();

    public async Task<List<T>> ToList()
    {
        var list = new List<T>();
        try
        {
            await ForEach(item => list.Add(item));
        }
        catch (Exception)
        {
            // TODO: If we ever allow multiple enumeration, this will need to be rethought
            foreach (var item in list)
            {
                (item as IDisposable)?.Dispose();
            }

            throw;
        }

        return list;
    }

#if NET6_0_OR_GREATER
    public async IAsyncEnumerable<T> AsAsyncEnumerable()
    {
        while (await Next() is { } item)
        {
            yield return item;
        }
    }
#endif

    public async Task ForEach(Action<T> action)
    {
        while (await Next() is { } item)
        {
            action(item);
        }
    }

    public async Task ForEach(Func<T, Task> action)
    {
        while (await Next() is { } item)
        {
            await action(item);
        }
    }

    private class EmptySource : AsyncSource<T>
    {
        public override Task<T?> Next() => Task.FromResult<T?>(null);
    }
}