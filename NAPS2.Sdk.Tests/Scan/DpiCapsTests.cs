using NAPS2.Scan;
using Xunit;

namespace NAPS2.Sdk.Tests.Scan;

public class DpiCapsTests
{
    [Fact]
    public void TestCommonValues()
    {
        var fullRange = DpiCaps.ForRange(50, 10000, 50);
        Assert.Equal(fullRange.CommonValues, [50, 100, 150, 200, 300, 400, 600, 800, 1200, 2400, 4800, 10000]);

        var partialRange = DpiCaps.ForRange(300, 1200, 50);
        Assert.Equal(partialRange.CommonValues, [300, 400, 600, 800, 1200]);

        var mismatchRange = DpiCaps.ForRange(240, 990, 50);
        Assert.Equal(mismatchRange.CommonValues, [240, 340, 440, 640, 840, 990]);
    }
}