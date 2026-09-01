using NAPS2.Scan.Internal.Wia;
using NAPS2.Wia;
using Xunit;

namespace NAPS2.Sdk.Tests.Scan;

public class WiaProtocolVersionParserTests
{
    [Theory]
    [InlineData(WiaVersion.Wia10, "1.0")]
    [InlineData(WiaVersion.Wia20, "2.0")]
    [InlineData(WiaVersion.Default, null)]     // Edge case: default/unspecified WIA version
    public void Parse_ReturnsExpectedProtocolVersion(WiaVersion wiaVersion, string expected)
    {
        var result = WiaProtocolVersionParser.Parse(wiaVersion);
        Assert.Equal(expected, result);
    }
}
