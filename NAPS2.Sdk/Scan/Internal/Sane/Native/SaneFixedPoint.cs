namespace NAPS2.Scan.Internal.Sane.Native;

/// <summary>
/// Converts between doubles and SANE_Fixed values (signed 32-bit integers scaled by 2^16), matching the SANE_FIX and
/// SANE_UNFIX macros.
/// </summary>
internal static class SaneFixedPoint
{
    private const double SCALE = 1 << 16;

    public static double ToDouble(int value) => value / SCALE;

    public static int ToFixed(double value)
    {
        double scaled = value * SCALE;
        if (!(scaled is > int.MinValue - 1.0 and < int.MaxValue + 1.0))
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, "Expected SANE fixed point");
        }
        // Truncates toward zero
        return (int) scaled;
    }
}
