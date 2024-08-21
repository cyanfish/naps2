using System.Runtime.InteropServices;

namespace NAPS2.Tools.Project.Targets;

public static class TargetsHelper
{
    public static string PackageName(this Platform platform) => platform switch
    {
        Platform.Win64 => "win-x64",
        Platform.Mac => "mac-univ",
        Platform.MacIntel => "mac-x64",
        Platform.MacArm => "mac-arm64",
        Platform.Linux => "linux-x64",
        Platform.LinuxArm => "linux-arm64",
        _ => throw new ArgumentException()
    };

    public static IEnumerable<BuildType> EnumerateBuildTargets(string? buildType)
    {
        return
            buildType?.ToLowerInvariant() switch
            {
                var x when string.IsNullOrEmpty(x) || x == "all" =>
                    new[] { BuildType.Debug, BuildType.Release, BuildType.Msi, BuildType.Zip },
                "debug" => new[] { BuildType.Debug },
                "release" => new[] { BuildType.Release },
                "msi" => new[] { BuildType.Msi },
                "zip" => new[] { BuildType.Zip },
                "sdk" => new[] { BuildType.Sdk },
                _ => Array.Empty<BuildType>()
            };
    }

    public static IEnumerable<PackageTarget> EnumeratePackageTargets() => EnumeratePackageTargets(null, null, false);

    public static IEnumerable<PackageTarget> EnumeratePackageTargets(string? packageTypeOpt, string? platformOpt,
        bool requireCompatiblePlatform, bool xCompile = false)
    {
        var targets = DoEnumeratePackageTargets(packageTypeOpt, platformOpt, requireCompatiblePlatform, xCompile).ToList();
        if (targets.Count == 0)
        {
            throw new Exception($"Invalid package/platform combination: {packageTypeOpt}/{platformOpt}");
        }
        return targets;
    }

    private static IEnumerable<PackageTarget> DoEnumeratePackageTargets(string? packageTypeOpt, string? platformOpt,
        bool requireCompatiblePlatform, bool xCompile)
    {
        packageTypeOpt = packageTypeOpt?.ToLowerInvariant() ?? "";
        platformOpt = platformOpt?.ToLowerInvariant() ?? "";

        foreach (var packageType in packageTypeOpt.Split("+"))
        {
            foreach (var platform in platformOpt.Split("+"))
            {
                bool allPkg = string.IsNullOrEmpty(packageType) || packageType == "all";
                bool allPlat = string.IsNullOrEmpty(platform) || platform == "all";
                if ((allPkg || packageType == "exe") && (!requireCompatiblePlatform || OperatingSystem.IsWindows()))
                {
                    if (allPlat || platform == "win" || platform == "win64")
                    {
                        yield return new PackageTarget(PackageType.Exe, Platform.Win64);
                    }
                }
                if ((allPkg || packageType == "msi") && (!requireCompatiblePlatform || OperatingSystem.IsWindows()))
                {
                    if (allPlat || platform == "win" || platform == "win64")
                    {
                        yield return new PackageTarget(PackageType.Msi, Platform.Win64);
                    }
                }
                if ((allPkg || packageType == "zip") && (!requireCompatiblePlatform || OperatingSystem.IsWindows()))
                {
                    if (allPlat || platform == "win64")
                    {
                        yield return new PackageTarget(PackageType.Zip, Platform.Win64);
                    }
                }
                if ((allPkg || packageType == "deb") && (!requireCompatiblePlatform || OperatingSystem.IsLinux()))
                {
                    if ((allPlat || platform == "linux") && (!requireCompatiblePlatform ||
                                                           xCompile ||
                                                           RuntimeInformation.OSArchitecture == Architecture.X64))
                    {
                        yield return new PackageTarget(PackageType.Deb, Platform.Linux);
                    }
                    if ((allPlat || platform == "linuxarm") && (!requireCompatiblePlatform ||
                                                              xCompile ||
                                                              RuntimeInformation.OSArchitecture == Architecture.Arm64))
                    {
                        yield return new PackageTarget(PackageType.Deb, Platform.LinuxArm);
                    }
                }
                if ((allPkg || packageType == "rpm") && (!requireCompatiblePlatform || OperatingSystem.IsLinux()))
                {
                    if ((allPlat || platform == "linux") && (!requireCompatiblePlatform ||
                                                           xCompile ||
                                                           RuntimeInformation.OSArchitecture == Architecture.X64))
                    {
                        yield return new PackageTarget(PackageType.Rpm, Platform.Linux);
                    }
                    if ((allPlat || platform == "linuxarm") && (!requireCompatiblePlatform ||
                                                              xCompile ||
                                                              RuntimeInformation.OSArchitecture == Architecture.Arm64))
                    {
                        yield return new PackageTarget(PackageType.Rpm, Platform.LinuxArm);
                    }
                }
                if ((allPkg || packageType == "flatpak") && (!requireCompatiblePlatform || OperatingSystem.IsLinux()))
                {
                    if ((allPlat || platform == "linux") && (!requireCompatiblePlatform ||
                                                           xCompile ||
                                                           RuntimeInformation.OSArchitecture == Architecture.X64))
                    {
                        yield return new PackageTarget(PackageType.Flatpak, Platform.Linux);
                    }
                    if ((allPlat || platform == "linuxarm") && (!requireCompatiblePlatform ||
                                                              xCompile ||
                                                              RuntimeInformation.OSArchitecture == Architecture.Arm64))
                    {
                        yield return new PackageTarget(PackageType.Flatpak, Platform.LinuxArm);
                    }
                }
                if ((allPkg || packageType == "pkg") && (!requireCompatiblePlatform || OperatingSystem.IsMacOS()))
                {
                    if ((allPlat || platform == "mac") && (!requireCompatiblePlatform ||
                                                           RuntimeInformation.OSArchitecture == Architecture.Arm64))
                    {
                        yield return new PackageTarget(PackageType.Pkg, Platform.Mac);
                    }
                    if (allPlat || platform == "macintel")
                    {
                        yield return new PackageTarget(PackageType.Pkg, Platform.MacIntel);
                    }
                    if ((allPlat || platform == "macarm") && (!requireCompatiblePlatform ||
                                                              RuntimeInformation.OSArchitecture == Architecture.Arm64))
                    {
                        yield return new PackageTarget(PackageType.Pkg, Platform.MacArm);
                    }
                }
            }
        }
    }

    public static IEnumerable<string?> GetBuildTypesFromPackageType(string? packageType)
    {
        return EnumeratePackageTargets(packageType, null, true).Select(x => (x.Type switch
        {
            PackageType.Msi => BuildType.Msi,
            PackageType.Zip => BuildType.Zip,
            _ => BuildType.Release
        }).ToString().ToLowerInvariant()).Distinct();
    }
}