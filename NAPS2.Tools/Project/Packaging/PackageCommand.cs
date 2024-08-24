using System.Text.RegularExpressions;
using NAPS2.Tools.Project.Targets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NAPS2.Tools.Project.Packaging;

public class PackageCommand : ICommand<PackageOptions>
{
    public int Run(PackageOptions opts)
    {
        foreach (var target in TargetsHelper.EnumeratePackageTargets(
                     opts.PackageType, opts.Platform, true, opts.XCompile))
        {
            PackageInfo GetPackageInfoForConfig() => GetPackageInfo(target.Platform, opts.Name);
            switch (target.Type)
            {
                case PackageType.Exe:
                    InnoSetupPackager.PackageExe(GetPackageInfoForConfig);
                    break;
                case PackageType.Msi:
                    WixToolsetPackager.PackageMsi(GetPackageInfoForConfig);
                    break;
                case PackageType.Zip:
                    ZipArchivePackager.PackageZip(GetPackageInfoForConfig);
                    break;
                case PackageType.Deb:
                    DebPackager.PackageDeb(GetPackageInfoForConfig(), opts.NoSign);
                    break;
                case PackageType.Rpm:
                    RpmPackager.PackageRpm(GetPackageInfoForConfig(), opts.NoSign);
                    break;
                case PackageType.Flatpak:
                    FlatpakPackager.Package(GetPackageInfoForConfig(), opts.NoPre);
                    break;
                case PackageType.Pkg:
                    MacPackager.Package(GetPackageInfoForConfig(), opts.NoSign, opts.NoNotarize);
                    break;
            }
        }
        return 0;
    }

    private static PackageInfo GetPackageInfo(Platform platform, string? packageName)
    {
        var pkgInfo = new PackageInfo(platform, ProjectHelper.GetCurrentVersionName(),
            ProjectHelper.GetCurrentVersion(), packageName);

        if (!platform.IsWindows())
        {
            // We rely on "dotnet publish" to build the installer
            return pkgInfo;
        }

        foreach (var project in new[]
                     { "NAPS2.App.WinForms", "NAPS2.App.Console" })
        {
            var buildPath = Path.Combine(Paths.SolutionRoot, project, "bin", "Release", "net9-windows", "win-x64",
                "publish");
            if (!Directory.Exists(buildPath))
            {
                throw new Exception($"Could not find build path.");
            }
            PopulatePackageInfo(buildPath, platform, pkgInfo);
        }

        var workerPath = Path.Combine(Paths.SolutionRoot, "NAPS2.App.Worker", "bin", "Release", "net9-windows",
            "win-x86", "publish");
        pkgInfo.AddFile(new PackageFile(workerPath, "lib", "NAPS2.Worker.exe"));

        var appBuildPath = Path.Combine(Paths.SolutionRoot, "NAPS2.App.WinForms", "bin", "Release", "net9-windows",
            "win-x64", "publish");
        AddPlatformFiles(pkgInfo, appBuildPath, "_win64");
        // Special case as we have a 64 bit main app and a 32 bit worker
        AddPlatformFile(pkgInfo, appBuildPath, "_win32", "twaindsm.dll");
        pkgInfo.AddFile(new PackageFile(appBuildPath, "", "appsettings.xml"));
        pkgInfo.AddFile(new PackageFile(Paths.SolutionRoot, "", "LICENSE", "license.txt"));
        pkgInfo.AddFile(new PackageFile(Paths.SolutionRoot, "", "CONTRIBUTORS", "contributors.txt"));
        return pkgInfo;
    }

    private static void PopulatePackageInfo(string buildPath, Platform platform, PackageInfo pkgInfo)
    {
        string[] excludeDlls =
        {
            // DLLs that are unneeded but missed by the built-in trimming
            "D3D",
            "Microsoft.DiaSymReader",
            "Microsoft.VisualBasic",
            "mscordaccore",
            "PenImc",
            "System.Data",
            "System.Private.DataContract",
            "System.Windows.Forms.Design",
            "System.Windows.Input",
            "System.Xaml",
            "UIAutomation",
            "WindowsBase",
            "wpfgfx"
        };

        var dir = new DirectoryInfo(buildPath);
        if (!dir.Exists)
        {
            throw new Exception($"Could not find path: {dir.FullName}");
        }

        // Parse the NAPS2.deps.json file to:
        // (a) As part of our effort to relocate DLLs under the "lib" subfolder for a cleaner install directory,
        //     get the map of subfolders where the prober will look for each DLL
        // (b) Strip out dependencies we're "manually" trimming via "excludeDlls"
        // TODO: Fix for NAPS2.Console
        var depsFile = dir.EnumerateFiles("*.deps.json").First();
        JObject deps;
        using (var stream = depsFile.OpenText())
        using (var reader = new JsonTextReader(stream))
            deps = (JObject) JToken.ReadFrom(reader);
        var targets = (JObject) deps["targets"]![".NETCoreApp,Version=v9.0/win-x64"]!;
        var dllMap = new Dictionary<string, string>();
        foreach (var pair in targets)
        {
            var pathPrefix = pair.Key;
            var target = (JObject) pair.Value!;
            if (target.TryGetValue("runtime", out var runtime))
            {
                foreach (var runtimeDlls in new Dictionary<string, JToken?>((JObject) runtime))
                {
                    var parts = runtimeDlls.Key.Split("/");
                    var dllName = parts.Last();
                    if (excludeDlls.Any(exclude => dllName.StartsWith(exclude)))
                    {
                        ((JObject) runtime).Remove(runtimeDlls.Key);
                    }
                    dllMap.Add(dllName, Path.Combine("lib", pathPrefix.Replace('/', Path.DirectorySeparatorChar), string.Join(Path.DirectorySeparatorChar, parts.SkipLast(1))));
                }
            }
            if (target.TryGetValue("resources", out var resources))
            {
                foreach (var runtimeDlls in new Dictionary<string, JToken?>((JObject) resources))
                {
                    var dllName = runtimeDlls.Key.Split("/").Last();
                    if (excludeDlls.Any(exclude => dllName.StartsWith(exclude)))
                    {
                        ((JObject) resources).Remove(runtimeDlls.Key);
                    }
                }
            }
            if (target.TryGetValue("native", out var native))
            {
                foreach (var runtimeDlls in new Dictionary<string, JToken?>((JObject) native))
                {
                    var dllName = runtimeDlls.Key.Split("/").Last();
                    if (excludeDlls.Any(exclude => dllName.StartsWith(exclude)))
                    {
                        ((JObject) native).Remove(runtimeDlls.Key);
                    }
                    if (!runtimeDlls.Key.StartsWith("host"))
                    {
                        dllMap.Add(runtimeDlls.Key,
                            Path.Combine("lib", pathPrefix.Replace('/', Path.DirectorySeparatorChar)));
                    }
                }
            }
        }
        using (StreamWriter file = depsFile.CreateText())
        using (JsonTextWriter writer = new JsonTextWriter(file) { Formatting = Formatting.Indented })
            deps.WriteTo(writer);

        string GetProbingPath(string dll)
        {
            if (dll is "NAPS2.dll" or "NAPS2.Console.dll") return "";
            return dllMap.GetValueOrDefault(dll, "");
        }

        string GetResourceProbingPath(string dll, string lang)
        {
            dll = dll.Replace(".resources.dll", ".dll");
            if (dllMap.TryGetValue(dll, out var path))
            {
                return Path.Combine(path, lang);
            }
            return lang;
        }

        // Add additionalProbingPaths=["lib"] to the NAPS2.runtimeconfig.json file
        // TODO: Fix for NAPS2.Console
        var runtimeConfigFile = dir.EnumerateFiles("*.runtimeconfig.json").First();
        JObject runtimeConfig;
        using (var stream = runtimeConfigFile.OpenText())
        using (var reader = new JsonTextReader(stream))
            runtimeConfig = (JObject) JToken.ReadFrom(reader);

        ((JObject) runtimeConfig["runtimeOptions"]!)["additionalProbingPaths"] = new JArray { "lib" };

        using (StreamWriter file = runtimeConfigFile.CreateText())
        using (JsonTextWriter writer = new JsonTextWriter(file) { Formatting = Formatting.Indented })
            runtimeConfig.WriteTo(writer);

        // Add each included file to the package contents
        foreach (var exeFile in dir.EnumerateFiles("*.exe"))
        {
            var dest = exeFile.Name.ToLower() switch
            {
                "naps2.worker.exe" => "lib",
                _ => ""
            };
            pkgInfo.AddFile(exeFile, dest);
        }
        foreach (var configFile in dir.EnumerateFiles("*.json"))
        {
            pkgInfo.AddFile(configFile, "");
        }
        foreach (var dllFile in dir.EnumerateFiles("*.dll"))
        {
            if (excludeDlls.All(exclude => !dllFile.Name.StartsWith(exclude)))
            {
                pkgInfo.AddFile(dllFile, GetProbingPath(dllFile.Name));
            }
        }
        foreach (var langFolder in dir.EnumerateDirectories()
                     .Where(x => Regex.IsMatch(x.Name, "[a-z]{2}(-[A-Za-z]+)?")))
        {
            foreach (var resourceDll in langFolder.EnumerateFiles("*.resources.dll"))
            {
                if (excludeDlls.All(exclude => !resourceDll.Name.StartsWith(exclude)))
                {
                    pkgInfo.AddFile(resourceDll, GetResourceProbingPath(resourceDll.Name, langFolder.Name));
                    pkgInfo.Languages.Add(langFolder.Name);
                }
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
        pkgInfo.AddFile(new PackageFile(Path.Combine(buildPath, platformPath), Path.Combine("lib", platformPath),
            fileName));
    }
}