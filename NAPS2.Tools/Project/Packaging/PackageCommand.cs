using System.Text.RegularExpressions;

namespace NAPS2.Tools.Project.Packaging;

public static class PackageCommand
{
    public static int Run(PackageOptions opts)
    {
        var platform = PlatformHelper.FromOption(opts.Platform, Platform.Win64);

        if (opts.What == "exe" || opts.What == "all")
        {
            // TODO: Allow customizing net version, platform, etc
            // TODO: The fact that we only have one project config for the app but multiple for the SDK is problematic; things will overwrite each other unless we either pull them explicitly from the right project or have a separate config or normalize things somehow to avoid needing multiple configs
            var pkgInfo = GetPackageInfo(platform, "InstallerEXE");
            InnoSetupPackager.PackageExe(pkgInfo, opts.Verbose);
        }
        if (opts.What == "msi" || opts.What == "all")
        {
            var pkgInfo = GetPackageInfo(platform, "InstallerMSI");
            WixToolsetPackager.PackageMsi(pkgInfo, opts.Verbose);
        }
        if (opts.What == "zip" || opts.What == "all")
        {
            var pkgInfo = GetPackageInfo(platform, "Standalone");
            ZipArchivePackager.PackageZip(pkgInfo, opts.Verbose);
        }
        return 0;
    }

    private static PackageInfo GetPackageInfo(Platform platform, string preferredConfig)
    {
        var pkgInfo = new PackageInfo(platform, ProjectHelper.GetProjectVersion("NAPS2.App.WinForms"));
        foreach (var project in new[]
                     { "NAPS2.Sdk", "NAPS2.Lib.Common", "NAPS2.App.Worker", "NAPS2.App.Console", "NAPS2.App.WinForms" })
        {
            var buildPath = Path.Combine(Paths.SolutionRoot, project, "bin", preferredConfig, "net462");
            if (!Directory.Exists(buildPath))
            {
                buildPath = Path.Combine(Paths.SolutionRoot, project, "bin", "Release", "net462");
            }
            if (!Directory.Exists(buildPath))
            {
                throw new Exception($"Could not find build path. Maybe run 'n2 build' first? {buildPath}");
            }
            PopulatePackageInfo(buildPath, platform, pkgInfo);
        }

        var appBuildPath = Path.Combine(Paths.SolutionRoot, "NAPS2.App.WinForms", "bin", "Release", "net462");
        if (platform == Platform.Win32)
        {
            AddPlatformFiles(pkgInfo, appBuildPath, "_win32");
        }
        else if (platform == Platform.Win64)
        {
            // Special case as we have a 64 bit main app and a 32 bit worker
            AddPlatformFile(pkgInfo, appBuildPath, "_win32", "NAPS2.Wia.Native.dll");
            AddPlatformFile(pkgInfo, appBuildPath, "_win64", "NAPS2.Wia.Native.dll");
            AddPlatformFile(pkgInfo, appBuildPath, "_win32", "twaindsm.dll");
            AddPlatformFile(pkgInfo, appBuildPath, "_win64", "twaindsm.dll");
            AddPlatformFile(pkgInfo, appBuildPath, "_win32", "pdfium.dll");
            AddPlatformFile(pkgInfo, appBuildPath, "_win64", "tesseract.exe");
        }
        else
        {
            throw new Exception("Unsupported platform");
        }
        pkgInfo.AddFile(new PackageFile(appBuildPath, "", "appsettings.xml"));
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
        foreach (var exeFile in dir.EnumerateFiles("*.exe"))
        {
            var dest = exeFile.Name.ToLower() switch
            {
                "naps2.worker.exe" => "lib",
                _ => ""
            };
            pkgInfo.AddFile(exeFile, dest);
        }
        foreach (var configFile in dir.EnumerateFiles("*.exe.config"))
        {
            var dest = configFile.Name.ToLower() switch
            {
                "naps2.worker.exe.config" => "lib",
                _ => ""
            };
            pkgInfo.AddFile(configFile, dest);
        }
        foreach (var dllFile in dir.EnumerateFiles("*.dll"))
        {
            // TODO: Blacklist unneeded dlls
            pkgInfo.AddFile(dllFile, "lib");
        }
        foreach (var langFolder in dir.EnumerateDirectories().Where(x => Regex.IsMatch(x.Name, "[a-z]{2}(-[A-Za-z]+)?")))
        {
            foreach (var resourceDll in langFolder.EnumerateFiles("*.resources.dll"))
            {
                pkgInfo.AddFile(resourceDll, Path.Combine("lib", langFolder.Name));
            }
        }
    }

    private static void AddPlatformFiles(PackageInfo pkgInfo, string buildPath, string platformPath)
    {
        var folder = new DirectoryInfo(Path.Combine(buildPath, platformPath));
        foreach (var file in folder.EnumerateFiles())
        {
            pkgInfo.AddFile(new PackageFile(file.DirectoryName ?? "", Path.Combine("lib", platformPath), file.Name));
        }
    }

    private static void AddPlatformFile(PackageInfo pkgInfo, string buildPath, string platformPath, string fileName)
    {
        pkgInfo.AddFile(new PackageFile(Path.Combine(buildPath, platformPath), Path.Combine("lib", platformPath), fileName));
    }
}