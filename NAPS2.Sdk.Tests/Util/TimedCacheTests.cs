using Xunit;

namespace NAPS2.Sdk.Tests.Util;

public class TimedCacheTests
{
    private readonly TimedCache<string, MyItem> _cache;
    private DateTime _now = DateTime.MinValue;
    private char _nextItemName = 'a';

    public TimedCacheTests()
    {
        _cache = new() { NowSource = () => _now };
    }

    [Fact]
    public void CachesItem()
    {
        var itemRef1 = _cache.UseCacheItem("a", CreateItem);
        var item1 = itemRef1.Value;
        var itemRef2 = _cache.UseCacheItem("a", CreateItem);
        var item2 = itemRef2.Value;

        Assert.Equal(item1, item2);
    }

    [Fact]
    public void PreservesItemWithinExpiry()
    {
        var itemRef1 = _cache.UseCacheItem("a", CreateItem);
        var item1 = itemRef1.Value;

        itemRef1.Dispose();
        _now += TimeSpan.FromSeconds(1);
        _cache.ExpireItems();

        Assert.False(item1.IsDisposed);

        _now += TimeSpan.FromSeconds(5);
        _cache.ExpireItems();

        Assert.True(item1.IsDisposed);
    }

    [Fact]
    public void PreservesItemWithActiveReferences()
    {
        var itemRef1 = _cache.UseCacheItem("a", CreateItem);
        var item1 = itemRef1.Value;
        var itemRef2 = _cache.UseCacheItem("a", CreateItem);
        var item2 = itemRef2.Value;
        Assert.Equal(item1, item2);

        itemRef1.Dispose();
        _now += TimeSpan.FromSeconds(10);
        _cache.ExpireItems();

        Assert.False(item1.IsDisposed);

        itemRef2.Dispose();
        _now += TimeSpan.FromSeconds(10);
        _cache.ExpireItems();

        Assert.True(item1.IsDisposed);
    }

    [Fact]
    public void MultipleItems()
    {
        var itemRef1 = _cache.UseCacheItem("a", CreateItem);
        var item1 = itemRef1.Value;
        var itemRef2 = _cache.UseCacheItem("b", CreateItem);
        var item2 = itemRef2.Value;

        Assert.Equal("a", item1.Name);
        Assert.Equal("b", item2.Name);

        itemRef1.Dispose();
        _now += TimeSpan.FromSeconds(10);
        _cache.ExpireItems();

        Assert.True(item1.IsDisposed);
        Assert.False(item2.IsDisposed);

        itemRef2.Dispose();
        _now += TimeSpan.FromSeconds(10);
        _cache.ExpireItems();

        Assert.True(item1.IsDisposed);
        Assert.True(item2.IsDisposed);
    }

    [Fact]
    public void ActiveReferenceRefreshesExpiry()
    {
        var itemRef1 = _cache.UseCacheItem("a", CreateItem);
        var item1 = itemRef1.Value;

        _now += TimeSpan.FromSeconds(10);
        itemRef1.Dispose();
        _cache.ExpireItems();

        Assert.False(item1.IsDisposed);

        _now += TimeSpan.FromSeconds(10);
        _cache.ExpireItems();

        Assert.True(item1.IsDisposed);
    }

    private MyItem CreateItem()
    {
        return new MyItem { Name = (_nextItemName++).ToString() };
    }

    private class MyItem : IDisposable
    {
        public required string Name { get; set; }
        public bool IsDisposed { get; private set; }
        public void Dispose() => IsDisposed = true;
    }
}