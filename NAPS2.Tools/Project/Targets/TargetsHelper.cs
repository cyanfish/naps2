using System.Runtime.InteropServices;

namespace NAPS2.Tools.Project.Targets;

public static class TargetsHelper
{
    public static string PackageName(this Platform platform) => platform switch
    {
        Platform.Win64 => "win-x64",
        Platform.WinArm64 => "win-arm64",
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

    public static IEnumerable<PackageTarget> EnumeratePackageTargets(TargetParams? targetParams = null)
    {
        targetParams ??= new TargetParams();
        var targets = DoEnumeratePackageTargets(targetParams).ToList();
        if (targets.Count == 0)
        {
            throw new Exception(
                $"Invalid package/platform combination: {targetParams.PackageType}/{targetParams.Platform}");
        }
        return targets;
    }

    private static IEnumerable<PackageTarget> DoEnumeratePackageTargets(TargetParams targetParams)
    {
        var packageTypeOpt = targetParams.PackageType?.ToLowerInvariant() ?? "";
        var platformOpt = targetParams.Platform?.ToLowerInvariant() ?? "";
        // Both building and running require the current OS to match the target
        bool requireOs = targetParams.RequireBuildable || targetParams.RequireRunnable;
        // Windows arm64 can be built on x64, but only run on arm64 (while x64 can be run on arm64 via emulation)
        bool winArmOk = !targetParams.RequireRunnable || RuntimeInformation.OSArchitecture == Architecture.Arm64;
        // Linux packages need a matching architecture, unless only building and cross-compiling is enabled
        bool LinuxArchOk(Architecture arch) => !requireOs ||
                                               (targetParams.XCompile && !targetParams.RequireRunnable) ||
                                               RuntimeInformation.OSArchitecture == arch;

        foreach (var packageType in packageTypeOpt.Split("+"))
        {
            foreach (var platform in platformOpt.Split("+"))
            {
                bool allPkg = string.IsNullOrEmpty(packageType) || packageType == "all";
                bool allPlat = string.IsNullOrEmpty(platform) || platform == "all";
                if ((allPkg || packageType == "exe") && (!requireOs || OperatingSystem.IsWindows()))
                {
                    if (allPlat || platform == "win" || platform == "win64")
                    {
                        yield return new PackageTarget(PackageType.Exe, Platform.Win64);
                    }
                    if ((allPlat || platform == "winarm") && winArmOk)
                    {
                        yield return new PackageTarget(PackageType.Exe, Platform.WinArm64);
                    }
                }
                if ((allPkg || packageType == "msi") && (!requireOs || OperatingSystem.IsWindows()))
                {
                    if (allPlat || platform == "win" || platform == "win64")
                    {
                        yield return new PackageTarget(PackageType.Msi, Platform.Win64);
                    }
                }
                if ((allPkg || packageType == "msix") && (!requireOs || OperatingSystem.IsWindows()))
                {
                    if (allPlat || platform == "win" || platform == "win64")
                    {
                        yield return new PackageTarget(PackageType.Msix, Platform.Win64);
                    }
                }
                if ((allPkg || packageType == "zip") && (!requireOs || OperatingSystem.IsWindows()))
                {
                    if (allPlat || platform == "win64")
                    {
                        yield return new PackageTarget(PackageType.Zip, Platform.Win64);
                    }
                }
                if ((allPkg || packageType == "deb") && (!requireOs || OperatingSystem.IsLinux()))
                {
                    if ((allPlat || platform == "linux") && LinuxArchOk(Architecture.X64))
                    {
                        yield return new PackageTarget(PackageType.Deb, Platform.Linux);
                    }
                    if ((allPlat || platform == "linuxarm") && LinuxArchOk(Architecture.Arm64))
                    {
                        yield return new PackageTarget(PackageType.Deb, Platform.LinuxArm);
                    }
                }
                if ((allPkg || packageType == "rpm") && (!requireOs || OperatingSystem.IsLinux()))
                {
                    if ((allPlat || platform == "linux") && LinuxArchOk(Architecture.X64))
                    {
                        yield return new PackageTarget(PackageType.Rpm, Platform.Linux);
                    }
                    if ((allPlat || platform == "linuxarm") && LinuxArchOk(Architecture.Arm64))
                    {
                        yield return new PackageTarget(PackageType.Rpm, Platform.LinuxArm);
                    }
                }
                if ((allPkg || packageType == "flatpak") && (!requireOs || OperatingSystem.IsLinux()))
                {
                    if ((allPlat || platform == "linux") && LinuxArchOk(Architecture.X64))
                    {
                        yield return new PackageTarget(PackageType.Flatpak, Platform.Linux);
                    }
                    if ((allPlat || platform == "linuxarm") && LinuxArchOk(Architecture.Arm64))
                    {
                        yield return new PackageTarget(PackageType.Flatpak, Platform.LinuxArm);
                    }
                }
                if ((allPkg || packageType == "pkg") && (!requireOs || OperatingSystem.IsMacOS()))
                {
                    if ((allPlat || platform == "mac") && (!requireOs ||
                                                           RuntimeInformation.OSArchitecture == Architecture.Arm64))
                    {
                        yield return new PackageTarget(PackageType.Pkg, Platform.Mac);
                    }
                    if (allPlat || platform == "macintel")
                    {
                        yield return new PackageTarget(PackageType.Pkg, Platform.MacIntel);
                    }
                    if ((allPlat || platform == "macarm") && (!requireOs ||
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
        return EnumeratePackageTargets(new TargetParams { PackageType = packageType, RequireBuildable = true })
            .Select(x => (x.Type switch
            {
                PackageType.Msi => BuildType.Msi,
                PackageType.Zip => BuildType.Zip,
                _ => BuildType.Release
            }).ToString().ToLowerInvariant()).Distinct();
    }
}