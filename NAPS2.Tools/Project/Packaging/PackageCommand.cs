using NAPS2.Tools.Project.Targets;

namespace NAPS2.Tools.Project.Packaging;

public class PackageCommand : ICommand<PackageOptions>
{
    public int Run(PackageOptions opts)
    {
        var appProject = ProjectHelper.GetWindowsAppProject(opts.App);
        foreach (var target in TargetsHelper.EnumeratePackageTargets(new TargetParams
                 {
                     PackageType = opts.PackageType,
                     Platform = opts.Platform,
                     RequireBuildable = true,
                     XCompile = opts.XCompile
                 }))
        {
            PackageInfo GetPackageInfoForConfig() => GetPackageInfo(target.Platform, appProject, opts.Name);
            switch (target.Type)
            {
                case PackageType.Exe:
                    InnoSetupPackager.PackageExe(GetPackageInfoForConfig, target.Platform, appProject, opts.NoSign);
                    break;
                case PackageType.Msi:
                    WixToolsetPackager.PackageMsi(GetPackageInfoForConfig, appProject, opts.NoSign);
                    break;
                case PackageType.Msix:
                    MsixPackager.PackageMsix(GetPackageInfoForConfig, appProject, opts.NoSign);
                    break;
                case PackageType.Zip:
                    ZipArchivePackager.PackageZip(GetPackageInfoForConfig, target.Platform, appProject, opts.NoSign);
                    break;
                case PackageType.Deb:
                    DebPackager.PackageDeb(GetPackageInfoForConfig(), opts.NoSign);
                    break;
                case PackageType.Rpm:
                    RpmPackager.PackageRpm(GetPackageInfoForConfig(), opts.NoSign);
                    break;
                case PackageType.Flatpak:
                    FlatpakPackager.Package(GetPackageInfoForConfig(), opts.NoPre, opts.NoSign);
                    break;
                case PackageType.Pkg:
                    MacPackager.Package(GetPackageInfoForConfig(), opts.NoSign, opts.NoNotarize);
                    break;
            }
        }
        return 0;
    }

    private static PackageInfo GetPackageInfo(Platform platform, string appProject, string? packageName)
    {
        var pkgInfo = new PackageInfo(platform, ProjectHelper.GetCurrentVersionName(),
            ProjectHelper.GetCurrentVersion(), packageName);

        if (!platform.IsWindows())
        {
            // We rely on "dotnet publish" to build the installer
            return pkgInfo;
        }

        string arch = platform == Platform.WinArm64 ? "win-arm64" : "win-x64";

        var appBuildPath = Path.Combine(Paths.SolutionRoot, appProject, "bin", "Release", "net11.0-windows",
            arch, "publish");
        PopulatePackageInfo(appBuildPath, platform, pkgInfo);

        // Include the 32-bit worker (on both x64 and arm64) for TWAIN support
        var workerBuildPath = Path.Combine(Paths.SolutionRoot, "NAPS2.App.Worker", "bin", "Release", "net11.0-windows",
            "win-x86");
        var workerPath = Path.Combine(workerBuildPath, "publish");
        pkgInfo.AddFile(new PackageFile(workerPath, "", "NAPS2.Worker.exe"));
        // The worker encodes scanned images as JPEG, so it needs its own (32-bit) copy of the encoder. This isn't in the
        // publish folder as the worker is published as a single file.
        AddPlatformFile(pkgInfo, workerBuildPath, "_win32", "naps2_jpeg.dll");

        if (platform == Platform.Win64)
        {
            AddPlatformFiles(pkgInfo, appBuildPath, "_win64");
            // Special case as we have a 64 bit main app and a 32 bit worker
            AddPlatformFile(pkgInfo, appBuildPath, "_win32", "twaindsm.dll");
        }
        if (platform == Platform.WinArm64)
        {
            AddPlatformFiles(pkgInfo, appBuildPath, "_winarm");
        }

        pkgInfo.AddFile(new PackageFile(Paths.SolutionRoot, "", "LICENSE", "license.txt"));
        pkgInfo.AddFile(new PackageFile(Paths.SolutionRoot, "", "CONTRIBUTORS", "contributors.txt"));

        return pkgInfo;
    }

    private static void PopulatePackageInfo(string buildPath, Platform platform, PackageInfo pkgInfo)
    {
        var dir = new DirectoryInfo(buildPath);
        if (!dir.Exists)
        {
            throw new Exception($"Could not find path: {dir.FullName}");
        }

        // NAPS2.exe is published as a single file. The other top-level files (NAPS2.Console.exe, appsettings.xml) are
        // the ones that aren't bundled into it. Native libraries in platform subfolders are added separately.
        foreach (var file in dir.EnumerateFiles())
        {
            pkgInfo.AddFile(file, "");
        }

        // Satellite resource assemblies are bundled into NAPS2.exe, but installers still need the list of languages
        var outputDir = dir.Parent ?? throw new InvalidOperationException();
        foreach (var langFolder in outputDir.EnumerateDirectories()
                     .Where(x => File.Exists(Path.Combine(x.FullName, "NAPS2.Lib.resources.dll"))))
        {
            pkgInfo.Languages.Add(langFolder.Name);
        }
    }

    private static void AddPlatformFiles(PackageInfo pkgInfo, string buildPath, string platformPath)
    {
        var folder = new DirectoryInfo(Path.Combine(buildPath, platformPath));
        foreach (var file in folder.EnumerateFiles())
        {
            pkgInfo.AddFile(new PackageFile(file.DirectoryName ?? "", platformPath, file.Name));
        }
    }

    private static void AddPlatformFile(PackageInfo pkgInfo, string buildPath, string platformPath, string fileName)
    {
        pkgInfo.AddFile(new PackageFile(Path.Combine(buildPath, platformPath), platformPath, fileName));
    }
}