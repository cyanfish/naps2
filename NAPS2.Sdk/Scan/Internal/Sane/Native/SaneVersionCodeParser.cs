namespace NAPS2.Scan.Internal.Sane.Native;

internal static class SaneVersionCodeParser
{
    /// <summary>
    /// Parses a SANE version code (as signed int) into a semantic version string.
    /// </summary>
    public static string Parse(int versionCode)
    {
        return Parse(unchecked((uint)versionCode));
    }

    /// <summary>
    /// Parses a SANE version code (as unsigned int) into a semantic version string.
    /// The version code format: (major << 24) | (minor << 16) | patch
    /// </summary>
    public static string Parse(uint versionCode)
    {
        uint major = (versionCode >> 24) & 0xff;
        uint minor = (versionCode >> 16) & 0xff;
        uint patch = versionCode & 0xffff;
        return $"{major}.{minor}.{patch}";
    }
}