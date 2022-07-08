namespace NAPS2.Tools.Project;

public static class PlatformHelper
{
    public static Platform FromOption(string? platformOpt, Platform defaultPlatform)
    {
        if (platformOpt == null)
        {
            return defaultPlatform;
        }
        return Enum.TryParse(typeof(Platform), platformOpt, true, out var platform)
            ? (Platform) platform!
            : throw new Exception("Invalid platform");
    }

    public static string PackageName(this Platform platform) => platform switch
    {
        Platform.Win32 => "win-x86",
        Platform.Win64 => "win-x64",
        Platform.Mac => "mac",
        Platform.MacArm => "mac-arm",
        Platform.Linux => "linux",
        _ => throw new ArgumentException()
    };
}