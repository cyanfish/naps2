using NAPS2.Util;
using Xunit;

namespace NAPS2.Sdk.Tests.Util;

public class NaturalStringComparerTests
{
    [Fact]
    public void Compare()
    {
        var comparer = new NaturalStringComparer();

        Assert.Equal(0, comparer.Compare("abc", "abc"));
        Assert.Equal(0, comparer.Compare("ABC", "abc"));

        Assert.True(comparer.Compare("aac", "abc") < 0);
        Assert.True(comparer.Compare("abc", "aac") > 0);

        Assert.True(comparer.Compare("acc", "abc") > 0);
        Assert.True(comparer.Compare("abc", "acc") < 0);

        Assert.True(comparer.Compare("a01", "a10") < 0);
        Assert.True(comparer.Compare("a10", "a01") > 0);

        Assert.True(comparer.Compare("a1b", "a10b") < 0);
        Assert.True(comparer.Compare("a10b", "a1b") > 0);

        Assert.True(comparer.Compare("a١b", "a١٠b") < 0);
        Assert.True(comparer.Compare("a١٠b", "a١b") > 0);
    }
}