using System.Runtime.InteropServices;

namespace NAPS2.Tools.Project.Targets;

public static class TargetsHelper
{
    public static string PackageName(this Platform platform) => platform switch
    {
        Platform.Win32 => "win-x86",
        Platform.Win64 => "win-x64",
        Platform.Mac => "mac-x64",
        Platform.MacArm => "mac-arm64",
        Platform.Linux => "linux-x64",
        Platform.LinuxArm32 => "linux-arm",
        Platform.LinuxArm64 => "linux-arm64",
        _ => throw new ArgumentException()
    };

    public static IEnumerable<Target> Enumerate(string? buildTypeOpt, string? platformOpt,
        TargetConstraints constraints)
    {
        if (string.IsNullOrEmpty(buildTypeOpt))
        {
            buildTypeOpt = "all";
        }
        if (string.IsNullOrEmpty(platformOpt))
        {
            platformOpt = constraints.AllowMultiplePlatforms ? "all" : GetBuildablePlatforms()[0].ToString();
        }

        string[] allowedBuildTypes = GetAllowedBuildTypes(constraints);
        string[] buildTypes = buildTypeOpt == "all" ? allowedBuildTypes : buildTypeOpt.Split("+");
        if (buildTypes.Any(x => !allowedBuildTypes.Contains(x)))
        {
            throw new Exception($"Invalid build type, expected one of {string.Join(",", allowedBuildTypes)}");
        }
        var buildTypesParsed = buildTypes.Select(ParseBuildType).ToList();

        string[] allowedPlatforms = (constraints.RequireBuildablePlatform ? GetBuildablePlatforms() : GetAllPlatforms())
            .Select(x => x.ToString().ToLowerInvariant()).ToArray();
        string[] platforms = platformOpt == "all" ? allowedPlatforms : platformOpt.Split("+");
        if (platforms.Any(x => !allowedPlatforms.Contains(x)))
        {
            throw new Exception($"Invalid platform, expected one of {string.Join(",", allowedPlatforms)}");
        }
        var platformsParsed = platforms.Select(ParsePlatform).ToList();

        if (!constraints.AllowMultiplePlatforms && platformsParsed.Count > 1)
        {
            throw new Exception("Only one platform can be specified");
        }

        foreach (var buildType in buildTypesParsed)
        {
            foreach (var platform in platformsParsed)
            {
                yield return new Target(buildType, platform);
            }
        }
    }

    private static string[] GetAllowedBuildTypes(TargetConstraints constraints)
    {
        if (OperatingSystem.IsWindows())
        {
            return constraints.InstallersOnly ? new[] { "exe", "msi" } :
                constraints.AllowDebug ? new[] { "debug", "exe", "msi", "zip" } :
                new[] { "exe", "msi", "zip" };
        }
        if (OperatingSystem.IsMacOS())
        {
            return new[] { "exe" };
        }
        if (OperatingSystem.IsLinux())
        {
            return new[] { "exe" };
        }
        throw new InvalidOperationException("Unsupported OS");
    }

    private static Platform[] GetAllPlatforms() =>
        new[]
        {
            Platform.Win64, Platform.Win32, Platform.MacArm, Platform.Mac, Platform.Linux, Platform.LinuxArm32,
            Platform.LinuxArm64
        };

    private static Platform[] GetBuildablePlatforms()
    {
        if (OperatingSystem.IsWindows()) return new[] { Platform.Win64, Platform.Win32 };
        if (OperatingSystem.IsMacOS())
        {
            return RuntimeInformation.OSArchitecture == Architecture.Arm64
                ? new[] { Platform.MacArm, Platform.Mac }
                : new[] { Platform.Mac };
        }
        if (OperatingSystem.IsLinux())
        {
            return new[] { Platform.Linux, Platform.LinuxArm64, Platform.LinuxArm32 };
        }
        throw new InvalidOperationException("Unsupported OS");
    }

    private static BuildType ParseBuildType(string value)
    {
        return Enum.TryParse(typeof(BuildType), value, true, out var buildType)
            ? (BuildType) buildType!
            : throw new Exception("Invalid build type");
    }

    public static Platform ParsePlatform(string value)
    {
        return Enum.TryParse(typeof(Platform), value, true, out var platform)
            ? (Platform) platform!
            : throw new Exception("Invalid platform");
    }
}