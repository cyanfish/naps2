using System;
using NAPS2.Util;
using Xunit;

namespace NAPS2.Sdk.Tests.Util;

public class CollectionExtensionsTests
{
    [Fact]
    public void IndicesOf()
    {
        var array = new[] { 'a', 'b', 'c', 'd', 'e' };
        Assert.Equal(new[] { 0, 1, 2, 3, 4 }, array.IndiciesOf(new[] { 'a', 'b', 'c', 'd', 'e' }));
        Assert.Equal(new[] { 4, 3, 2, 1, 0 }, array.IndiciesOf(new[] { 'e', 'd', 'c', 'b', 'a' }));
        Assert.Equal(new[] { 0, 4, 1 }, array.IndiciesOf(new[] { 'a', 'e', 'b' }));
        Assert.Equal(new int[] { }, array.IndiciesOf(new char[] { }));
        Assert.Throws<ArgumentException>(() => array.IndiciesOf(new [] { 'f' }));
    }
}