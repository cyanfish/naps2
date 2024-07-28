using NAPS2.Scan;
using Xunit;

namespace NAPS2.Sdk.Tests.Scan;

public class PerSourceCapsTests
{
    [Fact]
    public void UnionAllEmpty()
    {
        var emptyCaps = new PerSourceCaps();
        var union = PerSourceCaps.UnionAll([emptyCaps]);
        Assert.Null(union.DpiCaps);
        Assert.Null(union.BitDepthCaps);
        Assert.Null(union.PageSizeCaps);
    }

    [Fact]
    public void UnionAll()
    {
        var emptyCaps = new PerSourceCaps();
        var emptySubCaps = new PerSourceCaps
        {
            DpiCaps = new DpiCaps(),
            BitDepthCaps = new BitDepthCaps(),
            PageSizeCaps = new PageSizeCaps()
        };
        var caps1 = new PerSourceCaps
        {
            DpiCaps = new DpiCaps { Values = [100, 150] },
            BitDepthCaps = new BitDepthCaps { SupportsColor = true },
            PageSizeCaps = new PageSizeCaps { ScanArea = PageSize.Letter }
        };
        var caps2 = new PerSourceCaps
        {
            DpiCaps = new DpiCaps { Values = [50, 100] },
            BitDepthCaps = new BitDepthCaps { SupportsGrayscale = true },
            PageSizeCaps = new PageSizeCaps { ScanArea = PageSize.A4 }
        };

        var union = PerSourceCaps.UnionAll([emptyCaps, emptySubCaps, caps1, caps2]);

        Assert.Equal([50, 100, 150], union.DpiCaps!.Values);
        Assert.True(union.BitDepthCaps!.SupportsColor);
        Assert.True(union.BitDepthCaps!.SupportsGrayscale);
        Assert.False(union.BitDepthCaps!.SupportsBlackAndWhite);
        Assert.Equal(8.5m, union.PageSizeCaps!.ScanArea!.WidthInInches);
        Assert.Equal(297m, union.PageSizeCaps!.ScanArea!.HeightInMm, 3);
    }
}