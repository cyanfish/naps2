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

    public AsyncSource<T2> Map<T2>(Func<T, T2> mapper) where T2 : class
    {
        return new MappingSource<T2>(this, mapper);
    }

    private class EmptySource : AsyncSource<T>
    {
        public override Task<T?> Next() => Task.FromResult<T?>(null);
    }

    private class MappingSource<T2> : AsyncSource<T2> where T2 : class
    {
        private readonly AsyncSource<T> _original;
        private readonly Func<T, T2> _mapper;

        public MappingSource(AsyncSource<T> original, Func<T, T2> mapper)
        {
            _original = original;
            _mapper = mapper;
        }

        public override async Task<T2?> Next()
        {
            var item = await _original.Next();
            return item == null ? null : _mapper(item);
        }
    }
}