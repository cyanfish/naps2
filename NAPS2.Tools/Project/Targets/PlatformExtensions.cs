namespace NAPS2.Tools.Project.Targets;

public static class PlatformExtensions
{
    public static bool IsWindows(this Platform platform) => platform is Platform.Win64 or Platform.WinArm64;

    public static bool IsMac(this Platform platform) => platform is Platform.Mac or Platform.MacIntel or Platform.MacArm;

    public static bool IsLinux(this Platform platform) => platform is Platform.Linux or Platform.LinuxArm;
}