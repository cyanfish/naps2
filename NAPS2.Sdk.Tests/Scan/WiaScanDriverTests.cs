using NAPS2.Scan;
using NAPS2.Scan.Internal.Wia;
using NAPS2.Wia;
using Xunit;

namespace NAPS2.Sdk.Tests.Scan;

public class WiaScanDriverTests : ContextualTests
{
    private readonly WiaScanDriver _driver;

    public WiaScanDriverTests()
    {
        _driver = new WiaScanDriver(ScanningContext);
    }

    [Theory]
    [InlineData(WiaVersion.Wia10, "1.0")]
    [InlineData(WiaVersion.Wia20, "2.0")]
    [InlineData(WiaVersion.Default, null)]     // Edge case: default/unspecified WIA version
    public void GetWiaScanCaps_SetsProtocolVersionFromWiaVersion(WiaVersion wiaVersion, string expected)
    {
        var caps = _driver.GetWiaScanCaps("Naps2", "Scanner 3000", wiaVersion,
            false, false, false, null, null);

        Assert.Equal(expected, caps.MetadataCaps!.ProtocolVersion);
    }

    [Fact]
    public void GetWiaScanCaps_MapsManufacturerAndModel()
    {
        var caps = _driver.GetWiaScanCaps("Naps2", "Scanner 3000", WiaVersion.Wia20,
            false, false, false, null, null);

        Assert.Equal("Naps2", caps.MetadataCaps!.Manufacturer);
        Assert.Equal("Scanner 3000", caps.MetadataCaps!.Model);
    }

    [Fact]
    public void GetWiaScanCaps_OnlyIncludesCapsForSupportedPaperSources()
    {
        var flatbedCaps = new PerSourceCaps();
        var feederCaps = new PerSourceCaps();

        var caps = _driver.GetWiaScanCaps(null, null, WiaVersion.Wia20,
            true, true, true, flatbedCaps, feederCaps);

        Assert.True(caps.PaperSourceCaps!.SupportsFlatbed);
        Assert.True(caps.PaperSourceCaps!.SupportsFeeder);
        Assert.True(caps.PaperSourceCaps!.SupportsDuplex);
        Assert.Same(flatbedCaps, caps.FlatbedCaps);
        Assert.Same(feederCaps, caps.FeederCaps);
        Assert.Same(feederCaps, caps.DuplexCaps); // duplex reuses feeder caps
    }

    [Fact]
    public void GetWiaScanCaps_ExcludesCapsForUnsupportedPaperSources()
    {
        var flatbedCaps = new PerSourceCaps();
        var feederCaps = new PerSourceCaps();

        var caps = _driver.GetWiaScanCaps(null, null, WiaVersion.Wia20,
            false, false, false, flatbedCaps, feederCaps);

        Assert.False(caps.PaperSourceCaps!.SupportsFlatbed);
        Assert.False(caps.PaperSourceCaps!.SupportsFeeder);
        Assert.False(caps.PaperSourceCaps!.SupportsDuplex);
        Assert.Null(caps.FlatbedCaps);
        Assert.Null(caps.FeederCaps);
        Assert.Null(caps.DuplexCaps);
    }
}
