namespace NAPS2.Images.Mac;

/// <summary>
/// Building xamarin-mac and monomac on different platforms can mean dealing with different floating point types.
/// This class allows minimizing conditional compilation at the target site.
/// </summary>
public static class FloatHelper
{
#if MONOMAC
    public static float ToFloat(this float value)
    {
        return value;
    }

    public static double ToDouble(this double value)
    {
        return value;
    }

    public static float ToNFloat(this int value)
    {
        return value;
    }

    public static float ToNFloat(this float value)
    {
        return value;
    }

    public static double ToNDouble(this double value)
    {
        return value;
    }

    public static long ToNInt(this long value)
    {
        return value;
    }

    public static int ToNInt(this int value)
    {
        return value;
    }
#else
    public static float ToFloat(this NFloat value)
    {
        return (float) value.Value;
    }

    public static double ToDouble(this NFloat value)
    {
        return (double) value.Value;
    }

    public static NFloat ToNFloat(this int value)
    {
        return new NFloat(value);
    }

    public static NFloat ToNFloat(this float value)
    {
        return new NFloat(value);
    }

    public static NFloat ToNDouble(this double value)
    {
        return new NFloat(value);
    }

    public static nint ToNInt(this long value)
    {
        return (nint) value;
    }

    public static nint ToNInt(this int value)
    {
        return (nint) value;
    }
#endif
}