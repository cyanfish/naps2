using NAPS2.Escl;
using NAPS2.Scan.Internal.Escl;
using Xunit;

namespace NAPS2.Sdk.Tests.Scan;

public class EsclScanDriverTests : ContextualTests
{
    private readonly EsclScanDriver _driver;

    public EsclScanDriverTests()
    {
        _driver = new EsclScanDriver(ScanningContext);
    }

    [Fact]
    public void GetEsclScanCaps_MapsBasicMetadata()
    {
        var caps = new EsclCapabilities
        {
            Version = "2.6",
            MakeAndModel = "Test Scanner",
            Manufacturer = "Test Manufacturer",
            SerialNumber = "SN-12345"
        };

        var result = _driver.GetEsclScanCaps(caps, "http://example.com/icon.png");

        Assert.Equal("Test Scanner", result.MetadataCaps!.Model);
        Assert.Equal("Test Manufacturer", result.MetadataCaps!.Manufacturer);
        Assert.Equal("SN-12345", result.MetadataCaps!.SerialNumber);
        Assert.Equal("http://example.com/icon.png", result.MetadataCaps!.IconUri);
        Assert.Equal("2.6", result.MetadataCaps!.ProtocolVersion);
    }

    [Fact]
    public void GetEsclScanCaps_SetsProtocolVersionOnMetadataCaps_WhenVersionIsCustom()
    {
        var caps = new EsclCapabilities { Version = "2.0" };

        var result = _driver.GetEsclScanCaps(caps, null);

        Assert.Equal("2.0", result.MetadataCaps!.ProtocolVersion);
    }

    [Fact]
    public void GetEsclScanCaps_SetsDefaultProtocolVersionOnMetadataCaps_WhenVersionNotExplicitlySet()
    {
        // Version not set — EsclCapabilities.Version defaults to EsclCapabilities.DEFAULT_VERSION
        var caps = new EsclCapabilities();

        var result = _driver.GetEsclScanCaps(caps, null);

        Assert.Equal(EsclCapabilities.DEFAULT_VERSION, result.MetadataCaps!.ProtocolVersion);
    }

    [Fact]
    public void GetEsclScanCaps_SetsPaperSourceCaps()
    {
        var caps = new EsclCapabilities
        {
            PlatenCaps = new EsclInputCaps(),
            AdfSimplexCaps = new EsclInputCaps()
        };

        var result = _driver.GetEsclScanCaps(caps, null);

        Assert.True(result.PaperSourceCaps!.SupportsFlatbed);
        Assert.True(result.PaperSourceCaps!.SupportsFeeder);
        Assert.False(result.PaperSourceCaps!.SupportsDuplex);
        Assert.NotNull(result.FlatbedCaps);
        Assert.NotNull(result.FeederCaps);
        Assert.Null(result.DuplexCaps);
    }
}
