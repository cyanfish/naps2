#if !MAC
using NAPS2.Wia;

namespace NAPS2.Scan.Internal.Wia;

internal static class WiaProtocolVersionParser
{
    /// <summary>
    /// Parses a WiaVersion into the semantic protocol version string used for MetadataCaps.ProtocolVersion.
    /// Returns null if the version is not a recognized/explicit WIA version (e.g. WiaVersion.Default).
    /// </summary>
    public static string? Parse(WiaVersion wiaVersion)
    {
        return wiaVersion switch
        {
            WiaVersion.Wia10 => "1.0",
            WiaVersion.Wia20 => "2.0",
            _ => null
        };
    }
}
#endif
