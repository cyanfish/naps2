namespace NAPS2.Tools.Project.Targets;

public static class PlatformExtensions
{
    public static bool IsWindows(this Platform platform) => platform is Platform.Win32 or Platform.Win64;

    public static bool IsMac(this Platform platform) => platform is Platform.Mac or Platform.MacArm;

    public static bool IsLinux(this Platform platform) => platform is Platform.Linux or Platform.LinuxArm;
}