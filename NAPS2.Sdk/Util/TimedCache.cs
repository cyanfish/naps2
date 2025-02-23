using System.Threading;

namespace NAPS2.Util;

/// <summary>
/// Implements a cache where items are expired after a given timespan. Item references keep an item alive and refresh
/// its expiry time. Once an item has no references and expires, it will be disposed.
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TItem"></typeparam>
internal class TimedCache<TKey, TItem> : IDisposable where TKey : notnull
{
    private readonly Dictionary<TKey, Item> _items = new();
    private Timer? _timer;

    public TimeSpan ExpiryTime { get; init; } = TimeSpan.FromSeconds(5);

    public TimeSpan ExpiryCheckInterval { get; init; } = TimeSpan.FromSeconds(1);

    internal Func<DateTime> NowSource { get; init; } = () => DateTime.Now;

    public IItemRef UseCacheItem(TKey key, Func<TItem> itemFactory)
    {
        lock (this)
        {
            if (!_items.ContainsKey(key))
            {
                _items[key] = new Item { Value = itemFactory() };
                RefreshExpiry(_items[key]);
            }
            return new ItemRef(_items[key], this);
        }
    }

    private void RefreshExpiry(Item item)
    {
        item.Expiry = NowSource() + ExpiryTime;
        ResetTimer();
    }

    private void ResetTimer()
    {
        if (_timer == null && _items.Count > 0)
        {
            var interval = (int) ExpiryCheckInterval.TotalMilliseconds;
            _timer = new Timer(_ => ExpireItems(), null, interval, interval);
        }
        else if (_timer != null && _items.Count == 0)
        {
            _timer.Dispose();
            _timer = null;
        }
    }

    internal void ExpireItems()
    {
        lock (this)
        {
            foreach (var kvp in _items)
            {
                if (kvp.Value.RefCount == 0 && kvp.Value.Expiry < NowSource())
                {
                    _items.Remove(kvp.Key);
                    (kvp.Value.Value as IDisposable)?.Dispose();
                }
            }
            ResetTimer();
        }
    }

    public void Dispose()
    {
        lock (this)
        {
            foreach (var kvp in _items)
            {
                _items.Remove(kvp.Key);
                (kvp.Value.Value as IDisposable)?.Dispose();
            }
        }
    }

    private class Item
    {
        public required TItem Value { get; init; }

        public int RefCount { get; set; }

        public DateTime Expiry { get; set; }
    }

    private class ItemRef : IItemRef
    {
        private bool _disposed;

        public ItemRef(Item item, TimedCache<TKey, TItem> cache)
        {
            Item = item;
            Cache = cache;
            Item.RefCount++;
        }

        public Item Item { get; }
        public TimedCache<TKey, TItem> Cache { get; }

        public TItem Value => Item.Value;

        public void Dispose()
        {
            lock (Cache)
            {
                if (_disposed) return;
                _disposed = true;
                Item.RefCount--;
                Cache.RefreshExpiry(Item);
            }
        }
    }

    public interface IItemRef : IDisposable
    {
        public TItem Value { get; }
    }
}