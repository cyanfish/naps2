using NAPS2.Scan.Internal.Sane.Native;
using Xunit;

namespace NAPS2.Sdk.Tests.Scan;

public class SaneFixedPointTests
{
    [Theory]
    [InlineData(0.0, 0)]
    [InlineData(1.0, 0x10000)]
    [InlineData(12.5, 0xC8000)]
    [InlineData(0.5, 0x8000)]
    [InlineData(-1.0, -0x10000)]
    [InlineData(-12.5, -0xC8000)]
    [InlineData(-0.25, -0x4000)]
    [InlineData(-1.75, -0x1C000)]
    [InlineData(-100.0, -0x640000)]
    // Not exactly representable; truncates toward zero like SANE_FIX
    [InlineData(215.9, 14149222)]
    [InlineData(0.1, 6553)]
    [InlineData(-0.1, -6553)]
    [InlineData(355.6, 23304601)]
    [InlineData(-32768.0, int.MinValue)]
    [InlineData(32767.99999, int.MaxValue)]
    public void ToFixed(double value, int expected)
    {
        Assert.Equal(expected, SaneFixedPoint.ToFixed(value));
    }

    [Theory]
    [InlineData(0, 0.0)]
    [InlineData(0x10000, 1.0)]
    [InlineData(0x8000, 0.5)]
    [InlineData(-0xC8000, -12.5)]
    [InlineData(-0x4000, -0.25)]
    [InlineData(int.MinValue, -32768.0)]
    [InlineData(int.MaxValue, 32767.9999847412109375)]
    public void ToDouble(int value, double expected)
    {
        Assert.Equal(expected, SaneFixedPoint.ToDouble(value));
    }

    [Theory]
    [InlineData(-12.5)]
    [InlineData(-0.25)]
    [InlineData(215.9)]
    [InlineData(-100.3)]
    public void RoundTrip(double value)
    {
        Assert.Equal(value, SaneFixedPoint.ToDouble(SaneFixedPoint.ToFixed(value)), 4);
    }

    [Theory]
    [InlineData(32768.0)]
    [InlineData(-32768.1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void ToFixed_OutOfRange_Throws(double value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => SaneFixedPoint.ToFixed(value));
    }
}
