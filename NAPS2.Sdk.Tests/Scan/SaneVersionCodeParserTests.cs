using NAPS2.Scan.Internal.Sane.Native;
using Xunit;

namespace NAPS2.Sdk.Tests.Scan;

public class SaneVersionCodeParserTests
{
    [Theory]
    [InlineData((uint)0x0100001F, "1.0.31")]
    [InlineData((uint)0x0200000F, "2.0.15")]
    [InlineData((uint)0x0100001E, "1.0.30")]
    [InlineData(uint.MinValue, "0.0.0")]                    // Edge case: zero
    [InlineData(uint.MaxValue, "255.255.65535")]            // Edge case: uint.MaxValue
    public void Parse_UnsignedInt_ReturnsCorrectFormat(uint versionCode, string expected)
    {
        var result = SaneVersionCodeParser.Parse(versionCode);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(-1, "255.255.65535")]                       // Tests int overflow case
    [InlineData(int.MaxValue, "127.255.65535")]             // Edge case: int.MaxValue
    public void Parse_SignedInt_ConvertsAndReturnsCorrectFormat(int versionCode, string expected)
    {
        var result = SaneVersionCodeParser.Parse(versionCode);
        Assert.Equal(expected, result);
    }
}
