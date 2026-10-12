using System.Globalization;
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
    public void ParseInchesNoSpace()
    {
        var pageSize = PageSize.Parse("8.5x11in");
        Assert.NotNull(pageSize);
        Assert.Equal(8.5m, pageSize.Width);
        Assert.Equal(11m, pageSize.Height);
        Assert.Equal(PageSizeUnit.Inch, pageSize.Unit);
    }

    [Fact]
    public void ParseWellKnownLetter()
    {
        var pageSize = PageSize.Parse("Letter");
        Assert.NotNull(pageSize);
        Assert.Equal(8.5m, pageSize.Width);
        Assert.Equal(11m, pageSize.Height);
        Assert.Equal(PageSizeUnit.Inch, pageSize.Unit);
    }

    [Fact]
    public void ParseWellKnownA4()
    {
        var pageSize = PageSize.Parse("a4");
        Assert.NotNull(pageSize);
        Assert.Equal(210m, pageSize.Width);
        Assert.Equal(297m, pageSize.Height);
        Assert.Equal(PageSizeUnit.Millimetre, pageSize.Unit);
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

    [Fact]
    public void Centimetres()
    {
        Assert.Equal(21.59m, PageSize.Letter.WidthInCm);
        Assert.Equal(27.94m, PageSize.Letter.HeightInCm);
        Assert.Equal(21m, PageSize.A4.WidthInCm);
        Assert.Equal(29.7m, PageSize.A4.HeightInCm);
        var cmSize = new PageSize(21m, 29.7m, PageSizeUnit.Centimetre);
        Assert.Equal(21m, cmSize.WidthInCm);
        Assert.Equal(29.7m, cmSize.HeightInCm);
    }

    [Fact]
    public void ConvertInchesToMillimetres()
    {
        var pageSize = PageSize.Letter.ConvertTo(PageSizeUnit.Millimetre);
        Assert.Equal(215.9m, pageSize.Width);
        Assert.Equal(279.4m, pageSize.Height);
        Assert.Equal(PageSizeUnit.Millimetre, pageSize.Unit);
    }

    [Fact]
    public void ConvertMillimetresToCentimetres()
    {
        var pageSize = PageSize.A4.ConvertTo(PageSizeUnit.Centimetre);
        Assert.Equal(21m, pageSize.Width);
        Assert.Equal(29.7m, pageSize.Height);
        Assert.Equal(PageSizeUnit.Centimetre, pageSize.Unit);
    }

    [Fact]
    public void ConvertMillimetresToInches()
    {
        var pageSize = PageSize.A4.ConvertTo(PageSizeUnit.Inch);
        Assert.Equal(8.27m, Math.Round(pageSize.Width, 2));
        Assert.Equal(11.69m, Math.Round(pageSize.Height, 2));
        Assert.Equal(PageSizeUnit.Inch, pageSize.Unit);
    }

    [Fact]
    public void ConvertToSameUnit()
    {
        Assert.Equal(PageSize.A4, PageSize.A4.ConvertTo(PageSizeUnit.Millimetre));
    }

    [Theory]
    [InlineData("210", "297", PageSizeUnit.Millimetre, "210x297 mm")] // A4 exactly
    [InlineData("216", "279", PageSizeUnit.Millimetre, "8.5x11 in")] // Letter as rounded mm (e.g. from glibc)
    [InlineData("209.9", "297", PageSizeUnit.Millimetre, "210x297 mm")] // A4 from points (595 x 842 pt)
    [InlineData("216", "356", PageSizeUnit.Millimetre, "8.5x14 in")] // Legal as rounded mm
    [InlineData("8.5", "11", PageSizeUnit.Inch, "8.5x11 in")] // Letter exactly
    [InlineData("8.27", "11.69", PageSizeUnit.Inch, "210x297 mm")] // A4 as rounded inches
    [InlineData("21", "29.7", PageSizeUnit.Centimetre, "210x297 mm")] // A4 in cm
    public void FindWellKnownSize(string width, string height, PageSizeUnit unit, string expected)
    {
        var pageSize = PageSize.FindWellKnownSize(decimal.Parse(width, CultureInfo.InvariantCulture),
            decimal.Parse(height, CultureInfo.InvariantCulture), unit);
        Assert.Equal(expected, pageSize?.ToString());
    }

    [Theory]
    [InlineData("100", "150", PageSizeUnit.Millimetre)]
    [InlineData("297", "210", PageSizeUnit.Millimetre)] // A4 landscape
    [InlineData("4", "6", PageSizeUnit.Inch)]
    public void FindWellKnownSizeNoMatch(string width, string height, PageSizeUnit unit)
    {
        Assert.Null(PageSize.FindWellKnownSize(decimal.Parse(width, CultureInfo.InvariantCulture),
            decimal.Parse(height, CultureInfo.InvariantCulture), unit));
    }

    [Fact]
    public void SystemDefaultPageSize()
    {
        // The actual value depends on the system's settings, but if present it should be a well-known size
        var pageSize = PageSize.SystemDefault;
        Assert.Contains(pageSize, new[]
        {
            PageSize.Letter, PageSize.Legal, PageSize.A5, PageSize.A4, PageSize.A3, PageSize.B5, PageSize.B4
        });
    }

    [PlatformFact(include: PlatformFlags.Windows)]
    public void SystemDefaultPageSizeWindows()
    {
        // The user's locale should always have a known paper size
        Assert.NotNull(PlatformCompat.System.SystemDefaultPageSize);
    }

    [PlatformTheory(include: PlatformFlags.Windows)]
    [InlineData("en-US", "Letter")]
    [InlineData("en-CA", "Letter")]
    [InlineData("de-DE", "A4")]
    [InlineData("en-GB", "A4")]
    [InlineData("ja-JP", "A4")]
    public void GetPaperSizeWindows(string localeName, string expectedSize)
    {
        Assert.Equal(PageSize.Parse(expectedSize), WindowsSystemCompat.GetPaperSize(localeName));
    }

    [PlatformFact(include: PlatformFlags.Windows)]
    public void GetPaperSizeWindowsInvalidLocale()
    {
        Assert.Null(WindowsSystemCompat.GetPaperSize("xx-INVALID"));
    }

    [PlatformTheory(include: PlatformFlags.Linux)]
    [InlineData("C")]
    [InlineData("POSIX")]
    public void GetPaperSizeLinux(string localeName)
    {
        // Other locales may not be installed, but the C locale is always available and uses A4
        Assert.Equal(PageSize.A4, LinuxSystemCompat.GetPaperSize(localeName));
    }

    [PlatformFact(include: PlatformFlags.Linux)]
    public void GetPaperSizeLinuxInvalidLocale()
    {
        Assert.Null(LinuxSystemCompat.GetPaperSize("xx_INVALID.UTF-8"));
    }

    [PlatformFact(include: PlatformFlags.Mac)]
    public void SystemDefaultPageSizeMac()
    {
        // The user's locale should always have a known paper size
        Assert.NotNull(PlatformCompat.System.SystemDefaultPageSize);
    }

    [PlatformTheory(include: PlatformFlags.Mac)]
    [InlineData("en-US", "Letter")]
    [InlineData("en-CA", "Letter")]
    [InlineData("de-DE", "A4")]
    [InlineData("en-GB", "A4")]
    [InlineData("ja-JP", "A4")]
    public void GetPaperSizeMac(string localeId, string expectedSize)
    {
        Assert.Equal(PageSize.Parse(expectedSize), MacSystemCompat.GetPaperSize(localeId));
    }
}
