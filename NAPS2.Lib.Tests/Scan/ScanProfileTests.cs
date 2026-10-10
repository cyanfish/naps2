using NAPS2.Scan;
using Xunit;

namespace NAPS2.Lib.Tests.Scan;

public class ScanProfileTests
{
    // The actual value depends on the system's settings
    private static ScanPageSize ExpectedSystemDefault =>
        PageSize.SystemDefault == PageSize.A4 ? ScanPageSize.A4 : ScanPageSize.Letter;

    [Fact]
    public void SetSystemDefaultPageSize()
    {
        var profile = new ScanProfile { PageSize = ScanPageSize.SystemDefault };

        Assert.Equal(ExpectedSystemDefault, profile.PageSize);
    }

    [Fact]
    public void SetOtherPageSize()
    {
        var profile = new ScanProfile { PageSize = ScanPageSize.B5 };

        Assert.Equal(ScanPageSize.B5, profile.PageSize);
    }
}
