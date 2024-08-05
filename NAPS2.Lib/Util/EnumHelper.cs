namespace NAPS2.Util;

internal static class EnumHelper
{
    public static T ParseOrDefault<T>(string value) where T : struct
    {
        return Enum.TryParse<T>(value, out var result) ? result : default;
    }
}