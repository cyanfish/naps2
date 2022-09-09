namespace NAPS2.Scan.Internal.Sane.Native;

internal static class SaneFixedPoint
{
    public static double ToDouble(int value)
    {
        uint uvalue = (uint) value;
        short wholePart = (short) (uvalue >> 16);
        ushort fractionPart = (ushort) (uvalue & 0xFFFF);
        return wholePart + fractionPart / (double) ushort.MaxValue;
    }

    public static int ToFixed(double value)
    {
        if (value is >= short.MaxValue + 1 or < short.MinValue)
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, "Expected SANE fixed point");
        }
        short wholePart = (short) value;
        double fraction = value - wholePart;
        ushort fractionPart = (ushort) (fraction * ushort.MaxValue);
        return (wholePart << 16) | fractionPart;
    }
}