using Xunit;

namespace NAPS2.Sdk.Tests.Images;

public class PageSizeTests
{
    [Fact]
    public void ParseInches()
    {
        var pageSize = PageSize.Parse("8.5x11 in");
        Assert.NotNull(pageSize);
        Assert.Equal(8.5m, pageSize.Width);
        Assert.Equal(11m, pageSize.Height);
        Assert.Equal(PageSizeUnit.Inch, pageSize.Unit);
    }

    [Fact]
    public void ParseCentimetres()
    {
        var pageSize = PageSize.Parse("21x29.7 cm");
        Assert.NotNull(pageSize);
        Assert.Equal(21m, pageSize.Width);
        Assert.Equal(29.7m, pageSize.Height);
        Assert.Equal(PageSizeUnit.Centimetre, pageSize.Unit);
    }

    [Fact]
    public void ParseMillimetres()
    {
        var pageSize = PageSize.Parse("210x297 mm");
        Assert.NotNull(pageSize);
        Assert.Equal(210m, pageSize.Width);
        Assert.Equal(297m, pageSize.Height);
        Assert.Equal(PageSizeUnit.Millimetre, pageSize.Unit);
    }

    [Fact]
    public void ParseInvalid()
    {
        Assert.Null(PageSize.Parse("612x792 pt"));
        Assert.Null(PageSize.Parse("8,5x11 in"));
        Assert.Null(PageSize.Parse("8.5x11"));
        Assert.Null(PageSize.Parse("8.5 in"));
        Assert.Null(PageSize.Parse("8.5 11 in"));
        Assert.Null(PageSize.Parse(""));
        Assert.Null(PageSize.Parse(null));
    }

    [Fact]
    public void InchConversions()
    {
        var pageSize = new PageSize(8.5m, 11m, PageSizeUnit.Inch);
        Assert.Equal(8.5m, pageSize.WidthInInches);
        Assert.Equal(11m, pageSize.HeightInInches);
        Assert.Equal(8500, pageSize.WidthInThousandthsOfAnInch);
        Assert.Equal(11000, pageSize.HeightInThousandthsOfAnInch);
        Assert.Equal(215.9m, pageSize.WidthInMm);
        Assert.Equal(279.4m, pageSize.HeightInMm);
    }

    [Fact]
    public void CentimetreConversions()
    {
        var pageSize = new PageSize(21m, 29.7m, PageSizeUnit.Centimetre);
        Assert.Equal(8.2677m, pageSize.WidthInInches, 4);
        Assert.Equal(11.6929m, pageSize.HeightInInches, 4);
        Assert.Equal(210m, pageSize.WidthInMm);
        Assert.Equal(297m, pageSize.HeightInMm);
        Assert.Equal(8267, pageSize.WidthInThousandthsOfAnInch);
        Assert.Equal(11692, pageSize.HeightInThousandthsOfAnInch);
    }

    [Fact]
    public void MillimetreConversions()
    {
        var pageSize = new PageSize(210m, 297m, PageSizeUnit.Millimetre);
        Assert.Equal(8.2677m, pageSize.WidthInInches, 4);
        Assert.Equal(11.6929m, pageSize.HeightInInches, 4);
        Assert.Equal(210m, pageSize.WidthInMm);
        Assert.Equal(297m, pageSize.HeightInMm);
        Assert.Equal(8267, pageSize.WidthInThousandthsOfAnInch);
        Assert.Equal(11692, pageSize.HeightInThousandthsOfAnInch);
    }

    [Fact]
    public void InchesToString()
    {
        var pageSize = new PageSize(8.5m, 11m, PageSizeUnit.Inch);
        Assert.Equal("8.5x11 in", pageSize.ToString());
    }

    [Fact]
    public void CentimetresToString()
    {
        var pageSize = new PageSize(21m, 29.7m, PageSizeUnit.Centimetre);
        Assert.Equal("21x29.7 cm", pageSize.ToString());
    }

    [Fact]
    public void MillimetresToString()
    {
        var pageSize = new PageSize(210m, 297m, PageSizeUnit.Millimetre);
        Assert.Equal("210x297 mm", pageSize.ToString());
    }

    [Fact]
    public void ThousandsToString()
    {
        var pageSize = new PageSize(21000m, 29700m, PageSizeUnit.Millimetre);
        Assert.Equal("21000x29700 mm", pageSize.ToString());
    }
}