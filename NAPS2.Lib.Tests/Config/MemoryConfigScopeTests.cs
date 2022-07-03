using NAPS2.Config.Model;
using Xunit;

namespace NAPS2.Lib.Tests.Config;

public class MemoryConfigScopeTests
{
    [Fact]
    public void GetReturnsFalseForUnset()
    {
        var scope = new MemoryConfigScope<TestConfig>();
        Assert.False(scope.TryGet(c => c.UserName, out _));
        Assert.False(scope.TryGet(c => c.Sub.X, out _));
        Assert.False(scope.TryGet(c => c.Sub.Y, out _));
        Assert.False(scope.TryGet(c => c.Sub.SubSub.Val, out _));
    }

    [Fact]
    public void GetChildConfigThrows()
    {
        // Accessing a child config directly on the scope should fail
        var scope = new MemoryConfigScope<TestConfig>();
        Assert.Throws<ArgumentException>(() => scope.TryGet(c => c.Sub, out _));
    }

    [Fact]
    public void SetThenGetReturnsValue()
    {
        var scope = new MemoryConfigScope<TestConfig>();

        scope.Set(c => c.UserName, "blah");
        Assert.True(scope.TryGet(c => c.UserName, out var userName));
        Assert.Equal("blah", userName);

        scope.Set(c => c.Sub.X, 1);
        Assert.True(scope.TryGet(c => c.Sub.X, out var x));
        Assert.Equal(1, x);

        scope.Set(c => c.Sub.Y, null);
        Assert.True(scope.TryGet(c => c.Sub.Y, out var y));
        Assert.Null(y);

        scope.Set(c => c.Sub.SubSub.Val, "something");
        Assert.True(scope.TryGet(c => c.Sub.SubSub.Val, out var val));
        Assert.Equal("something", val);
    }

    [Fact]
    public void SetChildSetsAll()
    {
        var scope = new MemoryConfigScope<TestConfig>();
        scope.Set(c => c.Sub, new SubConfig
        {
            X = 2,
            Y = 3,
            SubSub = new SubSubConfig { Val = null }
        });

        Assert.True(scope.TryGet(c => c.Sub.X, out var x));
        Assert.Equal(2, x);
        Assert.True(scope.TryGet(c => c.Sub.Y, out var y));
        Assert.Equal(3, y);
        Assert.True(scope.TryGet(c => c.Sub.SubSub.Val, out var val));
        Assert.Null(val);
    }

    [Fact]
    public void SetRootSetsAll()
    {
        var scope = new MemoryConfigScope<TestConfig>();
        scope.Set(c => c, new TestConfig
        {
            UserName = "blah",
            Sub = new SubConfig
            {
                X = 2,
                Y = 3,
                SubSub = new SubSubConfig { Val = null }
            }
        });

        Assert.True(scope.TryGet(c => c.UserName, out var userName));
        Assert.Equal("blah", userName);
        Assert.True(scope.TryGet(c => c.Sub.X, out var x));
        Assert.Equal(2, x);
        Assert.True(scope.TryGet(c => c.Sub.Y, out var y));
        Assert.Equal(3, y);
        Assert.True(scope.TryGet(c => c.Sub.SubSub.Val, out var val));
        Assert.Null(val);
    }

    [Fact]
    public void GetReturnsFalseAfterRemove()
    {
        var scope = new MemoryConfigScope<TestConfig>();

        scope.Set(c => c.UserName, "blah");
        scope.Set(c => c.Sub.X, 1);
        scope.Set(c => c.Sub.Y, null);
        scope.Set(c => c.Sub.SubSub.Val, "something");

        Assert.True(scope.TryGet(c => c.UserName, out _));
        scope.Remove(c => c.UserName);
        Assert.False(scope.TryGet(c => c.UserName, out _));

        Assert.True(scope.TryGet(c => c.Sub.X, out _));
        Assert.True(scope.TryGet(c => c.Sub.Y, out _));
        Assert.True(scope.TryGet(c => c.Sub.SubSub.Val, out _));
        scope.Remove(c => c.Sub);
        Assert.False(scope.TryGet(c => c.Sub.X, out _));
        Assert.False(scope.TryGet(c => c.Sub.Y, out _));
        Assert.False(scope.TryGet(c => c.Sub.SubSub.Val, out _));
    }

    [Fact]
    public void RemoveWithIdentityRemovesAll()
    {
        var scope = new MemoryConfigScope<TestConfig>();

        scope.Set(c => c.UserName, "blah");
        scope.Set(c => c.Sub.X, 1);
        scope.Set(c => c.Sub.Y, null);
        scope.Set(c => c.Sub.SubSub.Val, "something");

        scope.Remove(c => c);

        Assert.False(scope.TryGet(c => c.UserName, out _));
        Assert.False(scope.TryGet(c => c.Sub.X, out _));
        Assert.False(scope.TryGet(c => c.Sub.Y, out _));
        Assert.False(scope.TryGet(c => c.Sub.SubSub.Val, out _));
    }

    [Fact]
    public void CopyFromCopiesSetProperties()
    {
        var scope = new MemoryConfigScope<TestConfig>();
        var source = new ConfigStorage<TestConfig>();

        scope.CopyFrom(source);

        Assert.False(scope.TryGet(c => c.UserName, out _));
        Assert.False(scope.TryGet(c => c.Sub.X, out _));
        Assert.False(scope.TryGet(c => c.Sub.Y, out _));
        Assert.False(scope.TryGet(c => c.Sub.SubSub.Val, out _));

        scope.Set(c => c.UserName, "blah");
        scope.Set(c => c.Sub.SubSub.Val, "something");
        scope.CopyFrom(source);

        Assert.True(scope.TryGet(c => c.UserName, out var userName));
        Assert.Equal("blah", userName);
        Assert.True(scope.TryGet(c => c.Sub.SubSub.Val, out var val));
        Assert.Equal("something", val);
        Assert.False(scope.TryGet(c => c.Sub.X, out _));
        Assert.False(scope.TryGet(c => c.Sub.Y, out _));

        source = new ConfigStorage<TestConfig>();
        scope.CopyFrom(source);
        Assert.True(scope.TryGet(c => c.UserName, out _));
        Assert.True(scope.TryGet(c => c.Sub.SubSub.Val, out _));
    }

    [Config]
    public class TestConfig
    {
        public string UserName { get; set; }

        [Config]
        public SubConfig Sub { get; set; } = new();
    }

    public record SubConfig
    {
        public int X { get; init; }

        public int? Y { get; init; }

        public SubSubConfig SubSub { get; set; }
    }

    [Config]
    public record SubSubConfig
    {
        public string Val { get; init; }
    }
}