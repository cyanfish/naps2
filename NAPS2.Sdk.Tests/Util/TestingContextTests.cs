using Xunit;

namespace NAPS2.Sdk.Tests.Util;

public class TestingContextTests
{
    // We can't really check for testing == false, but we can check the rest...

    [Fact]
    public void IsTesting()
    {
        Assert.True(TestingContext.IsTesting);
    }

    [Fact]
    public void NoStaticDefaults()
    {
        Assert.Throws<InvalidOperationException>(TestingContext.NoStaticDefaults);
    }
}