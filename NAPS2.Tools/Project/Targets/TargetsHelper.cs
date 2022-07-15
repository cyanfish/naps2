namespace NAPS2.Tools.Project.Targets;

public static class TargetsHelper
{
    public static string PackageName(this Platform platform) => platform switch
    {
        Platform.Win32 => "win-x86",
        Platform.Win64 => "win-x64",
        Platform.Mac => "mac",
        Platform.MacArm => "mac-arm",
        Platform.Linux => "linux",
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
            // TODO: Default value for xplat
            platformOpt = constraints.AllowMultiplePlatforms ? "all" : "win64";
        }

        string[] allowedBuildTypes =
            constraints.InstallersOnly ? new[] { "exe", "msi" } :
            constraints.AllowDebug ? new[] { "debug", "exe", "msi", "zip" } :
            new[] { "exe", "msi", "zip" };
        string[] buildTypes = buildTypeOpt == "all" ? allowedBuildTypes : buildTypeOpt.Split("+");
        if (buildTypes.Any(x => !allowedBuildTypes.Contains(x)))
        {
            throw new Exception($"Invalid build type, expected one of {string.Join(",", allowedBuildTypes)}");
        }
        var buildTypesParsed = buildTypes.Select(ParseBuildType).ToList();

        // TODO: Change for xplat
        string[] allowedPlatforms = new[] { "win32", "win64" };
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