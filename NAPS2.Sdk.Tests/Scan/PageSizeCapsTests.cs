using NAPS2.Scan;
using Xunit;

namespace NAPS2.Sdk.Tests.Scan;

public class PageSizeCapsTests
{
    [Fact]
    public void Fits()
    {
        var letter = new PageSizeCaps { ScanArea = PageSize.Letter };
        var legal = new PageSizeCaps { ScanArea = PageSize.Legal };
        var a4 = new PageSizeCaps { ScanArea = PageSize.A4 };
        // 11.6 inch height is slightly too small, so this tests the 1% margin
        var a4AndLetter = new PageSizeCaps { ScanArea = new PageSize(8.5m, 11.6m, PageSizeUnit.Inch) };

        Assert.True(letter.Fits(PageSize.Letter));
        Assert.False(letter.Fits(PageSize.Legal));

        Assert.True(legal.Fits(PageSize.Letter));
        Assert.True(legal.Fits(PageSize.Legal));

        Assert.True(a4.Fits(PageSize.A4));
        Assert.False(a4.Fits(PageSize.Letter));

        Assert.True(a4AndLetter.Fits(PageSize.A4));
        Assert.True(a4AndLetter.Fits(PageSize.Letter));
    }
}